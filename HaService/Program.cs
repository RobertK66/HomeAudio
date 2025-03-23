using GraphQL.Types;
using GraphQL.SystemTextJson;
using GraphQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;

namespace HaService {


    public class Ship {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Speed { get; set; }
    }

    public class ShipType : ObjectGraphType<Ship> {
        public ShipType() {
            Description = "Ein Raumschiff.";
            Field(x => x.Id).Description("The Id of the ship");
            Field(x => x.Name).Description("The name of the ship.");
            Field(x => x.Speed).Description("Speed in parsecs per second ;-).");
        }
    }

    public enum Episode {
        NEWHOPE = 4,
        EMPIRE = 5,
        JEDI = 6
    }

    public class Droid {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Earnings { get; set; }
        public decimal LastEarnings { get; set; }
        public decimal AllEarnings { get; set; }

        public List<Episode> AppearsIn { get; set; } = new List<Episode>();
    }



    public class DroidType : ObjectGraphType<Droid> {
        public DroidType() {
            Description = "Ja ein Droid halt.";
            Field(x => x.Id).Description("The Id of the Droid.");
            Field(x => x.Name).Description("The name of the Droid.");
            Field(x => x.Earnings).Description("The income over this year.");
            Field(x => x.LastEarnings).Description("The income over last year.");
            Field(x => x.AllEarnings).Description("The income sum.");
            Field(x => x.AppearsIn);
        }
    }

    public class StarWarsQuery : ObjectGraphType {
        public StarWarsQuery() {
            Field<DroidType>("hero").Resolve(context => NewMethod(context));
            Field<ListGraphType<ShipType>>("ships_slower_than").Argument<DecimalGraphType>("maxspeed").Resolve(GetShips);
        }

        private List<Ship> GetShips(IResolveFieldContext<object?> context) {
            var maxspeed = context.GetArgument<decimal>("maxspeed");
            
            List<Ship> retVal = new List<Ship>();

            if (maxspeed > 0.166M) {
                retVal.Add(new Ship() { Id = "3", Name = "Orion III", Speed = 1M/6 });
            }

            if (maxspeed > 0.34567M) {
                retVal.Add(new Ship() { Id = "1", Name = "Todesstern", Speed = 0.34567M });
            }
            if (maxspeed > 16.2M) {
                retVal.Add(new Ship() { Id = "2", Name = "Starkreuzer", Speed = 16.22234M });
            }
            return retVal;
        }

        private object? NewMethod(IResolveFieldContext<object?> context) {
            Droid retVal = new Droid {
                Id = "1",
                Name = "R2-D2",
                Earnings = 100M / 3
            };
            retVal.LastEarnings = 100M * 2 / 3;  // this gives 0.66...67! retVal.Earnings*2 -> 0.66..66 !!!!
            retVal.AllEarnings = retVal.Earnings + retVal.LastEarnings; // 100.0 now!
            retVal.AppearsIn.Add(Episode.NEWHOPE);
            retVal.AppearsIn.Add(Episode.JEDI);
            return retVal;
        }
    }

    public class StarWarsSchema : Schema {
        public StarWarsSchema() {
            Query = new StarWarsQuery();  
        }
    }

    public class Program {
        //public static async Task Main(string[] args) {
        public static void Main(string[] args) {

            //// hello GraphQL code
            //var schema = new StarWarsSchema();
            //var json = await schema.ExecuteAsync(_ =>
            //{
            //    _.Query = "{ hero { id name } }";
            //});
            //Console.WriteLine(json);
            // TODO: https://www.apollographql.com/blog/how-to-use-subscriptions-in-graphiql-1d6ab8dbd74b 


            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddGraphQL(options =>
            {
                options.AddSystemTextJson(options => {
                    // This is needed to get decimals with complete precision length....
                    options.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.WriteAsString | System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
                });
                options.AddSchema<StarWarsSchema>();
            });
            WebApplication app = builder.Build();
            app.UseWebSockets();                    // used for subscription only!?
            app.UseGraphQL("/graphql");             // url to host GraphQL endpoint in this server.

            app.UseGraphQLGraphiQL(                 // The test client running in the same server here.
                        "/",                        // url to host GraphiQL at
                        new GraphQL.Server.Ui.GraphiQL.GraphiQLOptions {
                            GraphQLEndPoint = "/graphql",           // url of GraphQL endpoint
                            //SubscriptionsEndPoint = "/graphql",   // url of GraphQL endpoint -> how to use subscription TODO....
                        });
            app.Run();
        }
    }
}
