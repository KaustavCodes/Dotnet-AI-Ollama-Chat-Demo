using Microsoft.Extensions.VectorData;

namespace VectorDataSearch
{
    public class Movie
    {
        [VectorStoreKey]
        public int Key { get; set; }

        [VectorStoreData]
        public required string Title { get; set; }

        [VectorStoreData]
        public required string Description { get; set; }

        [VectorStoreVector(Dimensions: 384, DistanceFunction = DistanceFunction.CosineSimilarity)]
        public float[] DescriptionVector { get; set; }

        public ReadOnlyMemory<float> Vector { get; set; }
    }

    public static class MovieData
    {
        public static Movie[] GetMovies()
        {
            return new Movie[]
            {
                new Movie
                {
                    Key = 1,
                    Title = "The Matrix",
                    Description = "A computer hacker learns about the true nature of reality and his role in the war against its controllers."
                },
                new Movie
                {
                    Key = 2,
                    Title = "Inception",
                    Description = "A thief who steals corporate secrets through dream-sharing technology is given the inverse task of planting an idea into the mind of a CEO."
                },
                new Movie
                {
                    Key = 3,
                    Title = "Interstellar",
                    Description = "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival."
                },
                new Movie
                {
                    Key = 4,
                    Title = "The Dark Knight",
                    Description = "When the menace known as the Joker emerges from his mysterious past, he wreaks havoc and chaos on the people of Gotham."
                },
                new Movie
                {
                    Key = 5,
                    Title = "Pulp Fiction",
                    Description = "The lives of two mob hitmen, a boxer, a gangster's wife, and a pair of diner bandits intertwine in four tales of violence and redemption."
                },
                new Movie
                {
                    Key = 6,
                    Title = "Shrek",
                    Description = "An ogre's quiet life is disrupted when numerous fairy tale characters are exiled to his swamp."
                },
                new Movie
                {
                    Key = 7,
                    Title = "American Pie",
                    Description = "A group of high school friends navigate the challenges of adolescence and their quest for love and popularity."
                },
                new Movie
                {
                    Key = 8,
                    Title = "Evil Dead",
                    Description = "A group of friends encounter a deadly force in a remote cabin, leading to a fight for survival."
                },
                // === Additional Movies (Genres: Sci-Fi, Action, Drama, Comedy, Horror, etc.) ===
                new Movie
                {
                    Key = 9,
                    Title = "The Shawshank Redemption",
                    Description = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency."
                },
                new Movie
                {
                    Key = 10,
                    Title = "Fight Club",
                    Description = "An insomniac office worker and a devil-may-care soap maker form an underground fight club that evolves into something much, much more."
                },
                new Movie
                {
                    Key = 11,
                    Title = "Forrest Gump",
                    Description = "The life story of a man with low IQ who unwittingly influences major historical events in the 20th century."
                },
                new Movie
                {
                    Key = 12,
                    Title = "The Lord of the Rings: The Fellowship of the Ring",
                    Description = "A meek hobbit from the Shire and eight companions set out on a journey to destroy the powerful One Ring and save Middle-earth."
                },
                new Movie
                {
                    Key = 13,
                    Title = "Parasite",
                    Description = "Greed and personal vendetta threaten the once-harmonious relationship between the rich Park family and the destitute Kim clan."
                },
                new Movie
                {
                    Key = 14,
                    Title = "Everything Everywhere All at Once",
                    Description = "An aging Chinese immigrant is swept up in an insane adventure, where she alone can save existence by exploring other universes."
                },
                new Movie
                {
                    Key = 15,
                    Title = "Dune: Part Two",
                    Description = "Paul Atreides unites with Chani and the Fremen while seeking revenge against conspirators who destroyed his family."
                },
                new Movie
                {
                    Key = 16,
                    Title = "Oppenheimer",
                    Description = "The story of American scientist J. Robert Oppenheimer and his role in the development of the atomic bomb."
                },
                new Movie
                {
                    Key = 17,
                    Title = "The Grand Budapest Hotel",
                    Description = "A young writer befriends the legendary concierge of the legendary hotel and becomes embroiled in a tale of theft and murder."
                },
                new Movie
                {
                    Key = 18,
                    Title = "Get Out",
                    Description = "A young African-American visits his white girlfriend's parents for the weekend, where his simmering uneasiness about their behavior escalates into pure terror."
                },
                new Movie
                {
                    Key = 19,
                    Title = "La La Land",
                    Description = "While navigating their careers in Los Angeles, a pianist and an actress fall in love while attempting to reconcile their aspirations for the future."
                },
                new Movie
                {
                    Key = 20,
                    Title = "Spider-Man: Into the Spider-Verse",
                    Description = "Teen Miles Morales becomes the Spider-Man of his universe and must join other Spider-People from across the Multiverse to stop a threat."
                },
                new Movie
                {
                    Key = 21,
                    Title = "Mad Max: Fury Road",
                    Description = "In a post-apocalyptic wasteland, a woman rebels against a tyrannical ruler in search for her homeland with the aid of a group of female prisoners and a drifter."
                },
                new Movie
                {
                    Key = 22,
                    Title = "The Prestige",
                    Description = "Rival magicians engage in competitive one-upmanship, each intent on besting the other in a battle of wits and illusions."
                },
                new Movie
                {
                    Key = 23,
                    Title = "Whiplash",
                    Description = "A young and talented drummer attends a prestigious music conservatory and is mentored by a ruthless instructor."
                },
                new Movie
                {
                    Key = 24,
                    Title = "Joker",
                    Description = "In Gotham City, mentally troubled comedian Arthur Fleck is disregarded and mistreated by society, leading him to become the criminal mastermind known as the Joker."
                },
                new Movie
                {
                    Key = 25,
                    Title = "The Godfather",
                    Description = "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son."
                },
                new Movie
                {
                    Key = 26,
                    Title = "Inglourious Basterds",
                    Description = "In Nazi-occupied France during World War II, a plan to assassinate Nazi leaders by a group of Jewish-American soldiers coincides with a theatre owner's vengeful plans."
                },
                new Movie
                {
                    Key = 27,
                    Title = "The Silence of the Lambs",
                    Description = "A young FBI cadet must receive the help of an incarcerated and manipulative cannibal killer to help catch another serial killer."
                },
                new Movie
                {
                    Key = 28,
                    Title = "Avengers: Endgame",
                    Description = "After the devastating events of Infinity War, the Avengers assemble once more to reverse Thanos' actions and restore order to the universe."
                },
                new Movie
                {
                    Key = 29,
                    Title = "The Lion King",
                    Description = "Lion prince Simba flees his kingdom only to learn the true meaning of responsibility and bravery."
                },
                new Movie
                {
                    Key = 30,
                    Title = "Deadpool",
                    Description = "A wisecracking mercenary gets experimented on and gains superpowers, but loses his skin in the process."
                },
                new Movie
                {
                    Key = 31,
                    Title = "3 Idiots",
                    Description = "Two friends embark on a quest to find their long-lost companion, learning valuable life lessons along the way about success, friendship, and following one's passion."
                },
                new Movie
                {
                    Key = 32,
                    Title = "Dangal",
                    Description = "A former wrestler trains his two daughters to become world-class wrestlers despite societal pressures."
                },
                new Movie
                {
                    Key = 33,
                    Title = "The Dark Knight Rises",
                    Description = "Eight years after the Joker's reign of chaos, Batman is forced out of exile to save Gotham from the terrorist Bane."
                },
                new Movie
                {
                    Key = 34,
                    Title = "Interstellar",
                    Description = "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival." // (duplicate title for testing if needed, or update as per your data model)
                },
                new Movie
                {
                    Key = 35,
                    Title = "The Banshees of Inisherin",
                    Description = "On a remote Irish island, two lifelong friends find themselves at an impasse when one abruptly ends their relationship."
                },
                new Movie
                {
                    Key = 36,
                    Title = "Barbie",
                    Description = "Barbie suffers a crisis that leads her to question her world and her existence, embarking on a journey of self-discovery."
                },
                new Movie
                {
                    Key = 37,
                    Title = "Top Gun: Maverick",
                    Description = "After thirty years, Maverick is tasked with training a detachment of Top Gun graduates for a specialized mission."
                },
                new Movie
                {
                    Key = 38,
                    Title = "RRR",
                    Description = "A fictitious story about two legendary Indian revolutionaries and their journey away from home in the 1920s."
                },
                new Movie
                {
                    Key = 39,
                    Title = "Poor Things",
                    Description = "The incredible tale about the fantastical evolution of Bella Baxter, a young woman brought back to life by a brilliant scientist."
                },
                new Movie
                {
                    Key = 40,
                    Title = "The Holdovers",
                    Description = "A cranky history teacher at a prep school is forced to remain on campus over Christmas break with a grieving student and the school's cook."
                }
            };
        }
    }
}