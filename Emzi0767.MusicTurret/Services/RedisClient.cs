// This file is a part of Music Turret project.
// 
// Copyright (C) 2018-2021 Emzi0767
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Emzi0767.MusicTurret.Data;
using StackExchange.Redis;

namespace Emzi0767.MusicTurret.Services
{
    /// <summary>
    /// Redis client. Provides a basic wrapper around Redis IO.
    /// </summary>
    public class RedisClient
    {
        private ConnectionMultiplexer Redis { get; }
        private TurretConfigRedis Configuration { get; }
        private IDatabase Database { get; }

        /// <summary>
        /// Creates a new Redis client.
        /// </summary>
        /// <param name="cfg"></param>
        public RedisClient(TurretConfigRedis cfg)
        {
            this.Configuration = cfg;
            this.Redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { new DnsEndPoint(this.Configuration.Hostname, this.Configuration.Port) },
                ClientName = "MusicTurret",
                Password = this.Configuration.Password,
                Ssl = this.Configuration.UseEncryption
            });
            this.Database = this.Redis.GetDatabase();
        }

        /// <summary>
        /// Retrieves the value of given property from Redis database.
        /// </summary>
        /// <typeparam name="TObj">Type of the object on which the property exists.</typeparam>
        /// <typeparam name="TProp">Type of the property.</typeparam>
        /// <param name="obj">Object the property of which to operate on.</param>
        /// <param name="expr">Property to retrieve the value of.</param>
        /// <param name="defaultValue">Default value for this property.</param>
        /// <returns></returns>
        public async Task GetValueForAsync<TObj, TProp>(TObj obj, Expression<Func<TObj, TProp>> expr, TProp defaultValue) where TObj : IIdentifiable
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            // get the expression body
            if (!(expr.Body is MemberExpression member))
                throw new ArgumentException("Invalid member expression.", nameof(expr));

            // get the property
            if (!(member.Member is PropertyInfo prop))
                throw new ArgumentException("Invalid member supplied.", nameof(expr));

            // construct the key and get its value
            var key = $"{obj.Identifier}:{prop.Name}";
            var val = await this.Database.StringGetAsync(key);

            // set the property value
            var t = typeof(TProp);
            object cvval;
            if (t.IsEnum)
                cvval = val.HasValue ? Enum.Parse(t, val) : defaultValue;
            else
                cvval = val.HasValue ? Convert.ChangeType((string)val, typeof(TProp)) : defaultValue;
            prop.SetValue(obj, cvval);
        }

        /// <summary>
        /// Stores the value of given property in Redis database.
        /// </summary>
        /// <typeparam name="TObj">Type of the object on which the property exists.</typeparam>
        /// <typeparam name="TProp">Type of the property.</typeparam>
        /// <param name="obj">Object the property of which to operate on.</param>
        /// <param name="expr">Property to store the value of.</param>
        /// <returns></returns>
        public async Task SetValueForAsync<TObj, TProp>(TObj obj, Expression<Func<TObj, TProp>> expr) where TObj : IIdentifiable
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            // get the expression body
            if (!(expr.Body is MemberExpression member))
                throw new ArgumentException("Invalid member expression.", nameof(expr));

            // get the property
            if (!(member.Member is PropertyInfo prop))
                throw new ArgumentException("Invalid member supplied.", nameof(expr));

            // get property value
            var cvval = prop.GetValue(obj).ToString();

            // construct the key and set its value
            var key = $"{obj.Identifier}:{prop.Name}";
            await this.Database.StringSetAsync(key, cvval);
        }
    }
}
