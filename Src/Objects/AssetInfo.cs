﻿namespace TickTrader.FDK.Objects
{
    using System.Globalization;

    /// <summary>
    /// This class has sense for cash accounts only
    /// </summary>
    public class AssetInfo
    {
        /// <summary>
        /// Creates a new empty instance of AssetInfo.
        /// </summary>
        public AssetInfo()
        {
        }

        /// <summary>
        /// Gets or sets asset currency.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets asset's balance.
        /// </summary>
        public double Balance { get; set; }

        /// <summary>
        /// Gets or sets asset's locked amount.
        /// </summary>
        public double LockedAmount { get; set; }

        /// <summary>
        /// Gets or sets asset's trade amount.
        /// </summary>
        public double TradeAmount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = string.Format("{0} = {1}/{2}/{3}", this.Currency, this.TradeAmount.ToString(CultureInfo.InvariantCulture), this.LockedAmount.ToString(CultureInfo.InvariantCulture), this.Balance.ToString(CultureInfo.InvariantCulture));
            return result;
        }
    }
}
