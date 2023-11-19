// See https://aka.ms/new-console-template for more information
// https://github.com/NetTopologySuite/NetTopologySuite/wiki/GettingStarted#invalid-geometries

using NetTopologySuite.Features;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

using (var correctedPolygonStream = File.OpenWrite("CorrectedPolygon.xml"))
{
    List<GeoAPI.Geometries.Coordinate> polygonCoordinates = new List<GeoAPI.Geometries.Coordinate>();
    StringBuilder correctedPolygonCoordinates = new StringBuilder();
    var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    var wktReader = new NetTopologySuite.IO.WKTReader(gf);

    XmlDocument xmlDocument = new XmlDocument();
    using (var referencePolygonStrean = File.OpenRead("Polygon.xml"))
    {
        if (referencePolygonStrean != null)
        {
            xmlDocument.Load(referencePolygonStrean);
        }
        var coordinates = xmlDocument.GetElementsByTagName("coordinates");
        if (coordinates?.Count > 0)
        {

            var coordinatesTemp = coordinates[0]?.InnerText;
            if (!string.IsNullOrEmpty(coordinatesTemp))
            {
                using (MemoryStream memoryStream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(coordinatesTemp)))
                {
                    using (StringReader reader = new StringReader(coordinatesTemp))
                    {
                        while (true)
                        {
                            var coordinateString = reader.ReadLine();
                            if (coordinateString == null)
                            {
                                break;
                            }

                            string[] tokens = coordinateString.Split(',');
                            if (tokens.Length == 3)
                            {
                                polygonCoordinates.Add(new GeoAPI.Geometries.Coordinate()
                                {
                                    X = Double.Parse(tokens[0]),
                                    Y = Double.Parse(tokens[1]),
                                    Z = Double.Parse(tokens[2])
                                });
                            }

                        }

                    }

                }

            }

        }

        var referencePolygon = gf.CreatePolygon(polygonCoordinates.ToArray());

        Console.WriteLine($"Reference Polygon - Valid: {referencePolygon.IsValid}");
        if (!referencePolygon.IsValid)
        {
            var validPolygon = referencePolygon.Buffer(0);
            Console.WriteLine($"Reference Polygon has been corrected - Valid: {validPolygon.IsValid}");
            foreach (var correctedCoordinate in validPolygon.Coordinates)
            {
                correctedPolygonCoordinates.AppendLine($"{correctedCoordinate.X},{correctedCoordinate.Y},{0}");
            }

            if (coordinates?.Count > 0)
            {
                coordinates[0].InnerText = correctedPolygonCoordinates.ToString();
                xmlDocument.Save(correctedPolygonStream);
            }

        }

    }

}


