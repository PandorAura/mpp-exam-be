using System.Collections.Concurrent;

namespace WebApplication1;

public class CharacterService
    {
        private readonly ConcurrentDictionary<int, Character> _characters = new();
        private int _nextId = 1;

        public CharacterService()
        {
            Add(new Character
            {
                Name = "Ahri",
                Role = "Mage",
                Image = "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Ahri_0.jpg",
                Attack = 6f,
                Defense = 4f
            });
            Add(new Character
            {
                Name = "Garen",
                Role = "Fighter",
                Image = "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Garen_0.jpg",
                Attack = 7.5f,
                Defense = 7f
            });
        }

        public IEnumerable<Character> GetAll() => _characters.Values;

        public Character? Get(int id) => _characters.TryGetValue(id, out var c) ? c : null;

        public Character Add(Character c)
        {
            c.Id = _nextId++;
            _characters[c.Id] = c;
            return c;
        }

        public bool Update(int id, Character updated)
        {
            if (!_characters.ContainsKey(id)) return false;
            updated.Id = id;
            _characters[id] = updated;
            return true;
        }

        public bool Delete(int id) => _characters.TryRemove(id, out _);
}

