using HeroExplorer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace HeroExplorer
{
    public class MarvelFacade
    {
        private const string privateKey = "0973b6641f5d28bfefe9e9539c57b2aea0d503bc";
        private const string publicKey = "359952a1f4ca6ec47fcac3e95b41814a";
        private const int MaxCharacters = 1500;

        private const string imageNotAvailablePath = "http://i.annihil.us/u/prod/marvel/i/mg/b/40/image_not_available";

        public static async Task PopulateMarvelCharactersAsync(ObservableCollection<Character> marvelCharaters)
        {
            try
            {
                var characterDataWrapper = await GetCharacterDataWrapperAsync();
                var characters = characterDataWrapper.data.results;
                foreach (var character in characters)
                {
                    //filter out characters that are missing thumbnail images
                    if (character.thumbnail != null && character.thumbnail.path != "" && character.thumbnail.path != imageNotAvailablePath)
                    {
                        character.thumbnail.small = string.Format("{0}/standard_small.{1}", character.thumbnail.path, character.thumbnail.extension);
                        character.thumbnail.large = string.Format("{0}/portrait_xlarge.{1}", character.thumbnail.path, character.thumbnail.extension);
                        marvelCharaters.Add(character);
                    }
                }
            }
            catch (Exception)
            {

                return;
            }
            

        }

        public static async Task PopulateMarvelComicsAsync(int characterId, ObservableCollection<ComicBook> marvelComics)
        {
            try
            {
                var comicDataWrapper = await GetComicDataWrapperAsync(characterId);
                var comics = comicDataWrapper.data.results;
                foreach (var comic in comics)
                {
                    //filter out characters that are missing thumbnail images
                    if (comic.thumbnail != null && comic.thumbnail.path != "" && comic.thumbnail.path != imageNotAvailablePath)
                    {
                        comic.thumbnail.small = string.Format("{0}/portrait_medium.{1}", comic.thumbnail.path, comic.thumbnail.extension);
                        comic.thumbnail.large = string.Format("{0}/portrait_xlarge.{1}", comic.thumbnail.path, comic.thumbnail.extension);
                        marvelComics.Add(comic);
                    }
                }
            }
            catch (Exception)
            {

                return;
            }


        }

        private static async Task<CharacterDataWrapper> GetCharacterDataWrapperAsync()
        {
            Random random = new Random();
            var offset = random.Next(MaxCharacters);

            string url = string.Format("http://gateway.marvel.com:80/v1/public/characters?limit=10&offset={0}", offset);
            //call out to marvel
            var jsonMessage = await CallMarvelAsync(url);

            //response -> string / json -> deserialize
            var serializer = new DataContractJsonSerializer(typeof(CharacterDataWrapper));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonMessage));

            var result = (CharacterDataWrapper) serializer.ReadObject(ms);

            return result;
        }

        private static async Task<ComicDataWrapper> GetComicDataWrapperAsync(int characterId)
        {
            var url = string.Format("http://gateway.marvel.com:80/v1/public/comics?characters={0}&limit=10", characterId);
            var jsonMessage = await CallMarvelAsync(url);
            //response -> string / json -> deserialize
            var serializer = new DataContractJsonSerializer(typeof(ComicDataWrapper));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonMessage));

            var result = (ComicDataWrapper)serializer.ReadObject(ms);

            return result;
        }

        private async static Task<string> CallMarvelAsync(string url)
        {
            // assemble url
            //get md5 hash
            var timestamp = DateTime.Now.Ticks.ToString();
            var hash = CreateHash(timestamp);
            string completeUrl = string.Format("{0}&apikey={1}&ts={2}&hash={3}", url, publicKey, timestamp, hash);
            //call out to marvel
            HttpClient http = new HttpClient();
            var response = await http.GetAsync(completeUrl);
            var jsonMessage = await response.Content.ReadAsStringAsync();

            return jsonMessage;
        }

        private static string CreateHash(string timestamp)
        {
            //var timestamp = DateTime.Now.Ticks.ToString();
            var toBeHashed = timestamp + privateKey + publicKey;

            return ComputeMD5(toBeHashed);
        }

        private static string ComputeMD5(string str)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            var res = CryptographicBuffer.EncodeToHexString(hashed);
            return res;
        }
    }
}
