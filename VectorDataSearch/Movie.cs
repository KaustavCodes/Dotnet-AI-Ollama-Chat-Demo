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
            };
        }
    }
}