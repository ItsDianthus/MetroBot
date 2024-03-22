using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModelsUtils
{
    public class DataProcessing
    {
        /// <summary>
        /// Initializes a new instance of the DataProcessing class.
        /// </summary>
        public DataProcessing() { }

        /// <summary>
        /// Sorts the array of TransportPoints by AvailableTransfer field in ascending order.
        /// </summary>
        /// <param name="transPoints">Array of TransportPoints to be sorted.</param>
        /// <returns>Sorted array of TransportPoints.</returns>
        public TransportPoints[] SortByAvailTransAscending(TransportPoints[] transPoints)
        {
            return transPoints.OrderBy(point => point.AvailableTransfer).ToArray();
        }

        /// <summary>
        /// Sorts the array of TransportPoints by YearOfComissioning field in descending order.
        /// </summary>
        /// <param name="transPoints">Array of TransportPoints to be sorted.</param>
        /// <returns>Sorted array of TransportPoints.</returns>
        public TransportPoints[] SortByYearDescending(TransportPoints[] transPoints)
        {
            return transPoints.OrderByDescending(point => point.YearOfComissioning).ToArray();
        }

        /// <summary>
        /// Filters the array of TransportPoints based on a specified keyword and field.
        /// </summary>
        /// <param name="transPoints">Array of TransportPoints to be filtered.</param>
        /// <param name="keyword">Keyword for filtering.</param>
        /// <param name="field">Field by which to filter.</param>
        /// <returns>Filtered array of TransportPoints.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified field does not exist.</exception>
        public TransportPoints[] Filter(TransportPoints[] transPoints, string keyword, string field)
        {
            switch(field)
            {
                case "District":
                    transPoints = transPoints.Where(station => station.District == keyword).ToArray();
                    break;
                case "CarCapacity":
                    transPoints = transPoints.Where(station => station.CarCapacity.ToString() == keyword).ToArray();
                    break;
                default:
                    throw new ArgumentException("Такого поля не существует");
            }
            return transPoints;
        }

        /// <summary>
        /// Filters the array of TransportPoints based on two status words.
        /// </summary>
        /// <param name="transPoints">Array of TransportPoints to be filtered.</param>
        /// <param name="statusWord">First status word for filtering.</param>
        /// <param name="nearStatWord">Second status word for filtering.</param>
        /// <returns>Filtered array of TransportPoints.</returns>
        public TransportPoints[] FilterByTwo(TransportPoints[] transPoints, string statusWord, string nearStatWord)
        {
            return transPoints.Where(station => station.Status == statusWord || station.NearStation == nearStatWord).ToArray();
        }
    }
}
