// See https://aka.ms/new-console-template for more information
// https://github.com/NetTopologySuite/NetTopologySuite/wiki/GettingStarted#invalid-geometries

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

using (var correctedLinearRingStream = File.OpenWrite("CorrectedLinearRing.xml"))
{
    List<Coordinate> linearRingCoordinates = new List<Coordinate>();
    StringBuilder correctedLinearRingCoordinates = new StringBuilder();
    var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    var wktReader = new NetTopologySuite.IO.WKTReader(gf);

    XmlDocument xmlDocument = new XmlDocument();
    using (var referenceLinearRingStrean = File.OpenRead("LinearRing.xml"))
    {
        if (referenceLinearRingStrean != null)
        {
            xmlDocument.Load(referenceLinearRingStrean);
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
                                linearRingCoordinates.Add(new Coordinate()
                                {
                                    X = Double.Parse(tokens[0]),
                                    Y = Double.Parse(tokens[1])
                                });
                            }

                        }

                    }

                }

            }

        }

        var referenceLinearRing = gf.CreateLinearRing(linearRingCoordinates.ToArray());

        Console.WriteLine($"Reference Linear Ring - Valid: {referenceLinearRing.IsValid}");
        Console.WriteLine($"Reference Linear Ring - CCW: {referenceLinearRing.IsCCW}");
        if (!referenceLinearRing.IsValid)
        {
            var validLinearRing = GeometryFixer.Fix(referenceLinearRing,false);
            Console.WriteLine($"Reference Linear Ring has been corrected - Valid: {validLinearRing.IsValid}");
            foreach (var correctedCoordinate in validLinearRing.Coordinates)
            {
                correctedLinearRingCoordinates.AppendLine($"{correctedCoordinate.X},{correctedCoordinate.Y},{0}");
            }

            if (coordinates?.Count > 0)
            {
                coordinates[0].InnerText = correctedLinearRingCoordinates.ToString();
                xmlDocument.Save(correctedLinearRingStream);
            }

        }

    }

}


