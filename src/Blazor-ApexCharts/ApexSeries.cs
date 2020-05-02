﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ApexCharts
{
    /// <summary>
    /// Adds a data series to the enclosing <see cref="Chart"/> component.
    /// </summary>
    public class ApexSeries<TItem> : ComponentBase, IDisposable where TItem : class
    {
        [CascadingParameter(Name = "Chart")] public ApexChart<TItem> Chart { get; set; }
        [Parameter] public string Name { get; set; }
        [Parameter] public Expression<Func<TItem, object>> XValue { get; set; }
        [Parameter] public Expression<Func<TItem, decimal>> YValue { get; set; }
        [Parameter] public Expression<Func<IEnumerable<TItem>, decimal>> YAggregate { get; set; }
        [Parameter] public Expression<Func<DataPoint, object>> OrderBy { get; set; }
        [Parameter] public Expression<Func<DataPoint, object>> OrderByDescending { get; set; }
        [Parameter] public bool ShowDataLabels { get; set; }
        [Parameter] public IEnumerable<TItem> Items { get; set; }
        private readonly Series series = new Series();
        protected override void OnParametersSet()
        {
            series.Name = Name;
            series.ShowDataLabels = ShowDataLabels;

            var xCompiled = XValue.Compile();
            IEnumerable<DataPoint> datalist;
            if (YAggregate == null)
            {
                var yCompiled = YValue.Compile();
                datalist = Items.Select(e => new DataPoint { X = xCompiled.Invoke(e), Y = yCompiled.Invoke(e), Items = new List<object> { e } });
            }
            else
            {
                var yAggCompiled = YAggregate.Compile();
                datalist = Items.GroupBy(e => xCompiled.Invoke(e)).Select(d => new DataPoint { X = d.Key, Y = yAggCompiled.Invoke(d), Items = d.ToList<object>() });
            }

            if (OrderBy != null)
            {
                datalist = datalist.OrderBy(o => OrderBy.Compile().Invoke(o));
            }
            else if(OrderByDescending != null)
            {
                datalist = datalist.OrderByDescending(o => OrderByDescending.Compile().Invoke(o));
            }

            series.Data = datalist;

          
        }

        protected override void OnInitialized()
        {
            if (Chart.Options.Series == null) { Chart.Options.Series = new List<Series>(); }
            Chart.Options.Series.Add(series);
        }


        void IDisposable.Dispose()
        {
            if (Chart.Options.Series != null && Chart.Options.Series.Contains(series))
            {
                Chart.Options.Series.Remove(series);
            }

        }

    }
}