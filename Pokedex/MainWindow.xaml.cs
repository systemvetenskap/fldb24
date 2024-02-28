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
        public MainWindow()
        {
            InitializeComponent();

            var vmPokemons = dbRepository.GetAllVmPokemons();

            foreach (var vmPokemon in vmPokemons)
            {
                Button newBtn = new Button
                {
                    Content = vmPokemon.Name,
                    Name = $"btn{vmPokemon.Name}",
                    Tag = vmPokemon
                };

                newBtn.Click += btnMenuClick;

                spMenu.Children.Add(newBtn);

                // Convert the color string to a Color object
                Color color;
                if (ColorConverter.ConvertFromString(vmPokemon.Color) is Color convertedColor)
                {
                    color = convertedColor;
                }
                else
                {
                    // Default color in case of an invalid string
                    color = Colors.LightGray;
                }

                // Set the background color
                newBtn.Background = new SolidColorBrush(color);
            }

            btnEvolveInto.Visibility = Visibility.Hidden;
            textblockArrow.Visibility = Visibility.Hidden;
        }

        private dbRepository dbRepository = new dbRepository();
        private Random random = new Random();
        private Pokemon currentPokemon;

        private async void btnMenuClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            var vmPokemon = (VmPokemon)button.Tag;

            if(vmPokemon is VmPokemon)
            {
                currentPokemon = await dbRepository.GetPokemon(vmPokemon.Id);
                drawPokemonData(currentPokemon);
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
                    drawPokemonData(currentPokemon);
                }
            }
        }



        private void drawPokemonData(Pokemon pokemon)
        {
            tbName.Text = pokemon.Name;
            tbDescription.Text = pokemon.Description;
            tbHeight.Text = pokemon.Height.ToString();
            tbWeight.Text = pokemon.Weight.ToString();
            tbGen.Text = pokemon.Generation.ToString();
            tbPokedexNumber.Text = pokemon.Id.ToString();

            if (pokemon.EvolvesInto is Pokemon)
            {
                btnEvolveInto.Visibility = Visibility.Visible;
                btnEvolveInto.Content = pokemon.EvolvesInto.Name;
                textblockArrow.Visibility = Visibility.Visible;
            }
            else
            {
                btnEvolveInto.Visibility = Visibility.Hidden;
                textblockArrow.Visibility = Visibility.Hidden;
            }

            imgPokemon.Source = new BitmapImage(new Uri(pokemon.ImageUrl));
        }

        private void btnEvolveInto_Click(object sender, RoutedEventArgs e)
        {
            currentPokemon = currentPokemon.EvolvesInto;
            drawPokemonData(currentPokemon);
        }

        private async void btnAddPokemon_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(tbHeight.Text, out double dbHeight))
            {
                if (int.TryParse(tbPokedexNumber.Text, out int pokemonId))
                {
                    if (double.TryParse(tbWeight.Text, out double dbWeight))
                    {
                        if (int.TryParse(tbGen.Text, out int intGeneration))
                        {
                            Pokemon pokemon = new Pokemon
                            {
                                Id = pokemonId,
                                Name = tbName.Text,
                                Description = tbDescription.Text,
                                Height = dbHeight,
                                Weight = dbWeight,
                                Color = tbColor.Text,
                                ImageUrl = tbImageURL.Text,
                                Generation = intGeneration
                            };

                            bool result = await dbRepository.AddPokemon(pokemon);
                        }
                    }
                }
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