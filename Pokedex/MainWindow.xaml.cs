using Pokedex.DAL;
using Pokedex.Models;
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
        }

        private dbRepository dbRepository = new dbRepository();
        private Random random = new Random();
        private Pokemon currentPokemon;

        private async void btnOk_Click(object sender, RoutedEventArgs e)
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

            if (pokemon.EvolvesInto is Pokemon)
            {
                btnEvolveInto.Visibility = Visibility.Visible;
                btnEvolveInto.Content = pokemon.EvolvesInto.Name;
            }
            else
            {
                btnEvolveInto.Visibility = Visibility.Hidden;
            }

            imgPokemon.Source = new BitmapImage(new Uri(pokemon.ImageUrl));
        }

        private void btnEvolveInto_Click(object sender, RoutedEventArgs e)
        {
            currentPokemon = currentPokemon.EvolvesInto;
            drawPokemonData(currentPokemon);
        }
    }
}