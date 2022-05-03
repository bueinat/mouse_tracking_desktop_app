using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.Views
{
    // source: https://rachel53461.wordpress.com/2011/09/17/wpf-grids-rowcolumn-count-properties/
    public class GridHelpers
    {
        #region RowCount Property

        /// <summary>
        /// Adds the specified number of Rows to RowDefinitions.
        /// Default Height is Auto
        /// </summary>
        public static readonly DependencyProperty RowCountProperty =
            DependencyProperty.RegisterAttached(
                "RowCount", typeof(int), typeof(GridHelpers),
                new PropertyMetadata(-1, RowCountChanged));

        // Get
        public static int GetRowCount(DependencyObject obj)
        {
            return (int)obj.GetValue(RowCountProperty);
        }

        // Set
        public static void SetRowCount(DependencyObject obj, int value)
        {
            obj.SetValue(RowCountProperty, value);
        }

        // Change Event - Adds the Rows
        public static void RowCountChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || (int)e.NewValue < 0)
                return;

            Grid grid = (Grid)obj;
            grid.RowDefinitions.Clear();

            for (int i = 0; i < (int)e.NewValue; i++)
                grid.RowDefinitions.Add(
                    new RowDefinition() { Height = GridLength.Auto });

            SetStarRows(grid);
        }

        #endregion RowCount Property

        #region ColumnCount Property

        /// <summary>
        /// Adds the specified number of Columns to ColumnDefinitions.
        /// Default Width is Auto
        /// </summary>
        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.RegisterAttached(
                "ColumnCount", typeof(int), typeof(GridHelpers),
                new PropertyMetadata(-1, ColumnCountChanged));

        // Get
        public static int GetColumnCount(DependencyObject obj)
        {
            return (int)obj.GetValue(ColumnCountProperty);
        }

        // Set
        public static void SetColumnCount(DependencyObject obj, int value)
        {
            obj.SetValue(ColumnCountProperty, value);
        }

        // Change Event - Add the Columns
        public static void ColumnCountChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || (int)e.NewValue < 0)
                return;

            Grid grid = (Grid)obj;
            grid.ColumnDefinitions.Clear();

            for (int i = 0; i < (int)e.NewValue; i++)
                grid.ColumnDefinitions.Add(
                    new ColumnDefinition() { Width = GridLength.Auto });

            SetStarColumns(grid);
        }

        #endregion ColumnCount Property

        #region StarRows Property

        /// <summary>
        /// Makes the specified Row's Height equal to Star.
        /// Can set on multiple Rows
        /// </summary>
        public static readonly DependencyProperty StarRowsProperty =
            DependencyProperty.RegisterAttached(
                "StarRows", typeof(string), typeof(GridHelpers),
                new PropertyMetadata(string.Empty, StarRowsChanged));

        // Get
        public static string GetStarRows(DependencyObject obj)
        {
            return (string)obj.GetValue(StarRowsProperty);
        }

        // Set
        public static void SetStarRows(DependencyObject obj, string value)
        {
            obj.SetValue(StarRowsProperty, value);
        }

        // Change Event - Makes specified Row's Height equal to Star
        public static void StarRowsChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || string.IsNullOrEmpty(e.NewValue.ToString()))
                return;

            SetStarRows((Grid)obj);
        }

        #endregion StarRows Property

        #region StarColumns Property

        /// <summary>
        /// Makes the specified Column's Width equal to Star.
        /// Can set on multiple Columns
        /// </summary>
        public static readonly DependencyProperty StarColumnsProperty =
            DependencyProperty.RegisterAttached(
                "StarColumns", typeof(string), typeof(GridHelpers),
                new PropertyMetadata(string.Empty, StarColumnsChanged));

        // Get
        public static string GetStarColumns(DependencyObject obj)
        {
            return (string)obj.GetValue(StarColumnsProperty);
        }

        // Set
        public static void SetStarColumns(DependencyObject obj, string value)
        {
            obj.SetValue(StarColumnsProperty, value);
        }

        // Change Event - Makes specified Column's Width equal to Star
        public static void StarColumnsChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || string.IsNullOrEmpty(e.NewValue.ToString()))
                return;

            SetStarColumns((Grid)obj);
        }

        #endregion StarColumns Property

        private static void SetStarColumns(Grid grid)
        {
            string[] starColumns =
                GetStarColumns(grid).Split(',');
            if (string.IsNullOrEmpty(starColumns[0]))
                return;

            if (starColumns.Length == 2)
            {
                int start = int.Parse(starColumns[0]);
                int end = int.Parse(starColumns[1]);
                for (int i = start; i <= end; i++)
                    grid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                foreach (string item in starColumns)
                {
                    if (int.Parse(item) < grid.ColumnDefinitions.Count)
                        grid.ColumnDefinitions[int.Parse(item)].Width = new GridLength(1, GridUnitType.Star);
                }
            }
        }

        private static void SetStarRows(Grid grid)
        {
            string[] starRows =
                GetStarRows(grid).Split(',');
            if (string.IsNullOrEmpty(starRows[0]))
                return;

            if (starRows.Length == 2)
            {
                int start = int.Parse(starRows[0]);
                int end = int.Parse(starRows[1]);
                for (int i = start; i <= end; i++)
                    grid.RowDefinitions[i].Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                foreach (string item in starRows)
                {
                    if (int.Parse(item) < grid.RowDefinitions.Count)
                        grid.RowDefinitions[int.Parse(item)].Height = new GridLength(1, GridUnitType.Star);
                }
            }
        }
    }
}