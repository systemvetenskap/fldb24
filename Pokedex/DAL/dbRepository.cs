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

        public List<VmPokemon> GetAllVmPokemons()
        {
            List<VmPokemon> vmPokemons = new List<VmPokemon>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var command = new NpgsqlCommand();

            StringBuilder sb = new StringBuilder("select p.id, p.name, c.name as color_name ");
            sb.AppendLine("from pokemon as p ");
            sb.AppendLine("join color as c on c.id=p.color_id ");
            sb.AppendLine("order by p.id");

            command.CommandText = sb.ToString();
            command.Connection = conn;

            using(var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    VmPokemon vm = new VmPokemon
                    {
                        Id = (int)reader["id"],
                        Name = reader["name"].ToString(),
                        Color = reader["color_name"].ToString()
                    };

                    vmPokemons.Add(vm);
                }
            }

            return vmPokemons;
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

        public async Task<bool> RemovePokemon(Pokemon pokemon)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();
                using var command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = $"delete from pokemon where id=@id";

                command.Parameters.AddWithValue("id", pokemon.Id);

                var affectedRows = await command.ExecuteNonQueryAsync();

                if(affectedRows == 0)
                    return false;
            }  
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        public async Task AddPokemon(Pokemon pokemon)
        {
            int? colorId = null;

            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();

                using var command1 = new NpgsqlCommand();
                command1.Connection = conn;
                command1.CommandText = $"select id from color where name=@color";

                command1.Parameters.AddWithValue("color", pokemon.Color);

                var task = command1.ExecuteScalarAsync();
                colorId = ConvertFromDBVal<int?>(task.Result);                

                using var transaction = await conn.BeginTransactionAsync();

                if(colorId is null)
                {
                    command1.CommandText = $"insert into color(name) values(@color) returning id";
                    command1.Parameters.AddWithValue("color", pokemon.Color);

                    var id = await command1.ExecuteScalarAsync();

                    if(id != null)
                    {
                        colorId = (int)id;
                    }
                }

                using var command = new NpgsqlCommand();

                command.Connection = conn;
                command.CommandText = $"insert into pokemon(id,name,weight,height,description,image_url,color_id,generation) values(@id,@name,@weight,@height,@description,@image_url,@color_id,@generation)";

                command.Parameters.AddWithValue("id", pokemon.Id);
                command.Parameters.AddWithValue("name", pokemon.Name);
                command.Parameters.AddWithValue("weight", pokemon.Weight);
                command.Parameters.AddWithValue("height", pokemon.Height);
                command.Parameters.AddWithValue("generation", pokemon.Generation);
                command.Parameters.AddWithValue("image_url", pokemon.ImageUrl);
                command.Parameters.AddWithValue("description", pokemon.Description);
                command.Parameters.AddWithValue("color_id", colorId);

                await command.ExecuteScalarAsync();
                await transaction.CommitAsync();
                     
            }
            catch (NpgsqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Pokemon?> GetPokemon(int id)
        {
            Pokemon? pokemon = null;
            int? evolvesInto = 0;

            using var conn = new NpgsqlConnection(_connectionString);

            conn.Open();

            using var command = new NpgsqlCommand();
            command.CommandText = $@"select p.*,c.name as color_name from pokemon as p join color as c on c.id=p.color_id where p.id=@id";
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
                        ImageUrl = reader["image_url"].ToString(),
                        Color = reader["color_name"].ToString()
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
