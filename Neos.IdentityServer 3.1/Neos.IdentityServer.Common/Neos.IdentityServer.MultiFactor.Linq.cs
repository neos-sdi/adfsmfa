//******************************************************************************************************************************************************************************************//
// Copyright (c) 2023 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

namespace Neos.IdentityServer.MultiFactor.Common
{
    #region Linq Predicate Builder Class
    public static class LinqFilter
    {
        /// <summary>
        /// True Method implementation
        /// </summary>
        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }

        /// <summary>
        /// False method implementation
        /// </summary>
        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        /// <summary>
        /// Or method implementation
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        /// <summary>
        /// And method implementation
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
    #endregion

    #region Linq Predicate Equality Class
    public class LinqEquality<T> : EqualityComparer<T>
    {
        private Func<T, T, bool> boolpredicate;

        public LinqEquality(Func<T, T, bool> predicate): base()
        {
            this.boolpredicate = predicate;
        }

        public override bool Equals(T x, T y)
        {
            if (x != null)
            {
                return ((y != null) && this.boolpredicate(x, y));
            }
            if (y != null)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(T obj)
        {
            return 0;
        }
    }
    #endregion

    #region LinqExtensions Class
    public static class LinqExtensions
    {
        public static void Set<T>(this IQueryable<T> source, Expression<Func<T, int>> property, int value = 0)
        {
            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;

            foreach (T Item in source)
            {
                propertyInfo.SetValue(Item, value, null);
            }
            return;
        }

        public static IQueryable<T> UniqueID<T>(this IQueryable<T> source, Expression<Func<T, int>> property, int counter = 1, int seed = 1)
        {
            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            var target = new List<T>(source);
            foreach (T Item in target)
            {
                propertyInfo.SetValue(Item, counter, null);
                counter = counter + seed;
            }
            return target.AsQueryable<T>();
        }

        public static IQueryable<T> Sort<T>(this IQueryable<T> source, string sortingColumn, bool isAscending)
        {
            if (String.IsNullOrEmpty(sortingColumn))
                return source;

            ParameterExpression parameter = Expression.Parameter(source.ElementType, String.Empty);
            MemberExpression property = Expression.Property(parameter, sortingColumn);
            LambdaExpression lambda = Expression.Lambda(property, parameter);

            string methodName = isAscending ? "OrderBy" : "OrderByDescending";
            Expression methodCallExpression = Expression.Call(typeof(Queryable), methodName, new Type[] { source.ElementType, property.Type }, source.Expression, Expression.Quote(lambda));
            return source.Provider.CreateQuery<T>(methodCallExpression);
        }
    }
    #endregion
}
