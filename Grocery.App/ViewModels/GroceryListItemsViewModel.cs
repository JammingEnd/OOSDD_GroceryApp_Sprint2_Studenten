using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id)) MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            //Maak de lijst AvailableProducts leeg
            //Haal de lijst met producten op
            //Controleer of het product al op de boodschappenlijst staat, zo niet zet het in de AvailableProducts lijst
            //Houdt rekening met de voorraad (als die nul is kun je het niet meer aanbieden).

            AvailableProducts.Clear();

            AvailableProducts = new ObservableCollection<Product>(_productService.GetAll()
                .Where(x => x.Stock > 0));

            foreach (var item in MyGroceryListItems)
            {
                if (AvailableProducts.Contains(item.Product))
                {
                    AvailableProducts.Remove(item.Product);
                }
            }

        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }
        [RelayCommand]
        public void AddProduct(Product product)
        {
            //Controleer of het product bestaat en dat de Id > 0
            //Maak een GroceryListItem met Id 0 en vul de juiste productid en grocerylistid
            //Voeg het GroceryListItem toe aan de dataset middels de _groceryListItemsService
            //Werk de voorraad (Stock) van het product bij en zorg dat deze wordt vastgelegd (middels _productService)
            //Werk de lijst AvailableProducts bij, want dit product is niet meer beschikbaar
            //call OnGroceryListChanged(GroceryList);

            if (product.Id <= 0)
            {
                return;
            }

            GroceryListItem newGrocery = new GroceryListItem(0, GetAvaibleId(), product.Id, product.Stock);

            _groceryListItemsService.Add(newGrocery);

            product.Stock--;
            _productService.Update(product);

            if (product.Stock <= 0)
            {
                AvailableProducts.Remove(product);
            }
            if (product.Stock > 0 && AvailableProducts.Where(x => x.Id == product.Id).First() != null)
            {
                AvailableProducts.Add(product);
            }
            OnGroceryListChanged(GroceryList);

        }

        private int GetAvaibleId()
        {
            int highestId = 0;
            foreach(var item in MyGroceryListItems)
            {
                if (item.GroceryListId > highestId)
                {
                    highestId = item.GroceryListId;
                }
            }
            return highestId + 1;
        }

        
    }
}
