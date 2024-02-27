using Microsoft.Extensions.Configuration;
using Npgsql;
using Pokedex.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokedex.DAL
{
    internal class dbRepository
    {
        private readonly string _connectionString;

        public dbRepository()
        {
           var config = new ConfigurationBuilder()
                                .AddUserSecrets<dbRepository>()
                                .Build();
           _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<int>> GetPokemonIDs()
        {
            List<int> pokemonIds = new List<int>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var command = new NpgsqlCommand();
            command.Connection = conn;
            command.CommandText = $"select id from pokemon";

            try
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var pokemonID = (int)reader["id"];
                        pokemonIds.Add(pokemonID);
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }             

            return pokemonIds;
        }

        public async Task<Pokemon?> GetPokemon(int id)
        {
            Pokemon? pokemon = null;
            int? evolvesInto = 0;

            using var conn = new NpgsqlConnection(_connectionString);

            conn.Open();

            using var command = new NpgsqlCommand();
            command.CommandText = $@"select * from pokemon where id=@id";
            command.Connection = conn;

            command.Parameters.AddWithValue("id", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync()) 
                {
                    pokemon = new Pokemon
                    {
                        Id = (int)reader["id"],
                        Name = reader["name"].ToString(),
                        Weight = (double)reader["weight"],
                        Height = (double)reader["height"],
                        Generation = (int)reader["generation"],
                        Description = reader["description"].ToString(),
                        ImageUrl = reader["image_url"].ToString()                        
                    };

                    evolvesInto = ConvertFromDBVal<int?>(reader["evolves_into_id"]);
                }
            }

            conn.Close();

            if (evolvesInto is int)
            {
                var nextPokemon = await GetPokemon((int)evolvesInto);
                pokemon.EvolvesInto = nextPokemon;
            }

            return pokemon;
        }

        private static T? ConvertFromDBVal<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default; // returns the default value for the type
            }
            return (T)obj;
        }

        private static object ConvertToDBVal<T>(object obj)
        {
            if (obj == null || obj == string.Empty)
            {
                return DBNull.Value;
            }
            return (T)obj;
        }

    }
}
