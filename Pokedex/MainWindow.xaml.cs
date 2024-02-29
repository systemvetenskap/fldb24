using Pokedex.DAL;
using Pokedex.Models;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pokedex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private dbRepository dbRepository = new dbRepository();
        private Random random = new Random();
        private Pokemon currentPokemon;
        private string defaultImgUrl = "https://static.wikia.nocookie.net/pokemon-fano/images/6/6f/Poke_Ball.png";

        public MainWindow()
        {
            InitializeComponent();

            fillMenuWithPokemonButtons();

            btnEvolveInto.Visibility = Visibility.Hidden;
            textblockArrow.Visibility = Visibility.Hidden;
        }

        private async void btnMenuClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            var vmPokemon = (VmPokemon)button.Tag;

            if(vmPokemon is VmPokemon)
            {
                currentPokemon = await dbRepository.GetPokemon(vmPokemon.Id);
                drawPokemonData();
            }
        }

        private void fillMenuWithPokemonButtons()
        {
            var vmPokemons = dbRepository.GetAllVmPokemons();
            spMenu.Children.Clear();

            foreach (var vmPokemon in vmPokemons)
            {
                Button newBtn = new Button
                {
                    Content = $" #{vmPokemon.Id.ToString("D3")} {vmPokemon.Name}",
                    Name = $"btn{vmPokemon.Name.Replace(" ", "")}",
                    Tag = vmPokemon,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Width = 130
                };

                //Add generic click event to button
                newBtn.Click += btnMenuClick;

                //Try setting the color from the color name found in vmPokemon
                Color color;
                if (ColorConverter.ConvertFromString(vmPokemon.Color) is Color convertedColor)
                    color = convertedColor;
                else
                    color = Colors.LightGray;
                             
                newBtn.Background = new SolidColorBrush(color);

                // Add the button to the stackpanel
                spMenu.Children.Add(newBtn);
            }
        }

        private async void btnSlumpa_Click(object sender, RoutedEventArgs e)
        {
            //int randomId = random.Next(1,10);

            var pokemonIDs = await dbRepository.GetPokemonIDs();

            if (pokemonIDs.Any())
            {
                int randomIndex = random.Next(1, (int)pokemonIDs.Count);

                int randomId = pokemonIDs[randomIndex];

                currentPokemon = await dbRepository.GetPokemon(randomId);

                if (currentPokemon is Pokemon)
                {
                    drawPokemonData();
                }
            }
        }



        private void drawPokemonData()
        {
            tbName.Text = currentPokemon.Name;
            tbDescription.Text = currentPokemon.Description;
            tbHeight.Text = currentPokemon.Height.ToString();
            tbWeight.Text = currentPokemon.Weight.ToString();
            tbGen.Text = currentPokemon.Generation.ToString();
            tbPokedexNumber.Text = currentPokemon.Id.ToString();
            tbColor.Text = currentPokemon.Color;

            if (currentPokemon.EvolvesInto is Pokemon)
            {
                btnEvolveInto.Visibility = Visibility.Visible;
                btnEvolveInto.Content = currentPokemon.EvolvesInto.Name;
                textblockArrow.Visibility = Visibility.Visible;
            }
            else
            {
                btnEvolveInto.Visibility = Visibility.Hidden;
                textblockArrow.Visibility = Visibility.Hidden;
            }

            if(!string.IsNullOrWhiteSpace(currentPokemon.ImageUrl))
                imgPokemon.Source = new BitmapImage(new Uri(currentPokemon.ImageUrl));
            else
                imgPokemon.Source = new BitmapImage(new Uri(defaultImgUrl));
        }

        private void btnEvolveInto_Click(object sender, RoutedEventArgs e)
        {
            currentPokemon = currentPokemon.EvolvesInto;
            drawPokemonData();
        }

        private async void btnAddPokemon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (double.TryParse(tbHeight.Text, out double dbHeight) &&
                int.TryParse(tbPokedexNumber.Text, out int pokemonId) &&
                double.TryParse(tbWeight.Text, out double dbWeight) &&
                int.TryParse(tbGen.Text, out int intGeneration))
                {
                    Pokemon pokemon = new Pokemon
                    {
                        Id = pokemonId,
                        Name = tbName.Text,
                        Description = tbDescription.Text,
                        Height = dbHeight,
                        Weight = dbWeight,
                        Color = tbColor.Text.ToLower(),
                        ImageUrl = tbImageURL.Text,
                        Generation = intGeneration
                    };

                    await dbRepository.AddPokemon(pokemon);

                    MessageBox.Show($"{pokemon.Name} är nu sparad till databasen.");
                    currentPokemon = pokemon;
                    drawPokemonData();
                    fillMenuWithPokemonButtons();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte lägga till din Pokémon i databasen pga: {ex.Message}");
            }
                      
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = await dbRepository.RemovePokemon(currentPokemon);
            if (result == false)
                MessageBox.Show("Det där gick åt skogen");
            else
                MessageBox.Show($"Pokemon {currentPokemon.Name} är nu borta ur databasen.");
        }
    }
}