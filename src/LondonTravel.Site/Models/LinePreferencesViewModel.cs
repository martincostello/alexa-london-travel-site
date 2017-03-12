// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A class representing the view model for a user's line preferences.
    /// </summary>
    public class LinePreferencesViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinePreferencesViewModel"/> class.
        /// </summary>
        public LinePreferencesViewModel()
        {
            AllLines = new List<FavoriteLineItem>();
        }

        /// <summary>
        /// Gets or sets the ETag associated with the preferences.
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets a value indicating whether a user has selected any favorite lines.
        /// </summary>
        public bool HasFavourites => FavouriteLines.Any((p) => p.IsFavorite);

        /// <summary>
        /// Gets the number of favorite lines the user has selected.
        /// </summary>
        public int SelectedLineCount => FavouriteLines.Count();

        /// <summary>
        /// Gets or sets all the line(s).
        /// </summary>
        public IList<FavoriteLineItem> AllLines { get; set; }

        /// <summary>
        /// Gets the user's favorite line(s).
        /// </summary>
        public IEnumerable<FavoriteLineItem> FavouriteLines => AllLines.Where((p) => p.IsFavorite);

        /// <summary>
        /// Gets the other line(s).
        /// </summary>
        public IEnumerable<FavoriteLineItem> OtherLines => AllLines.Where((p) => !p.IsFavorite);

        /// <summary>
        /// Gets or sets a value indicating whether the preferences were successfully updated.
        /// </summary>
        public bool? UpdateResult { get; set; }
    }
}
