using System;
using System.Collections.Generic;

namespace KufarDataInsights
{
    public class Apartment
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Apartment(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    public class ApartmentFinder
    {
        public List<Apartment> FindApartmentsInPolygon(List<Apartment> apartments, List<(double Latitude, double Longitude)> polygon)
        {
            if (polygon.Count < 3)
                throw new ArgumentException("Необходимо минимум три точки для создания фигуры.");

            var result = new List<Apartment>();

            foreach (var apartment in apartments)
            {
                if (IsPointInPolygon(apartment.Latitude, apartment.Longitude, polygon))
                {
                    result.Add(apartment);
                }
            }

            return result;
        }

        private bool IsPointInPolygon(double latitude, double longitude, List<(double Latitude, double Longitude)> polygon)
        {
            bool isInside = false;
            int n = polygon.Count;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if ((polygon[i].Latitude > latitude) != (polygon[j].Latitude > latitude) &&
                    (longitude < (polygon[j].Longitude - polygon[i].Longitude) * (latitude - polygon[i].Latitude) / (polygon[j].Latitude - polygon[i].Latitude) + polygon[i].Longitude))
                {
                    isInside = !isInside;
                }
            }

            return isInside;
        }
    }
}
