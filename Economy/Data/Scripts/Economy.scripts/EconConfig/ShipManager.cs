namespace Economy.scripts.EconConfig
{
    using System;
    using System.Linq;
    using Economy.scripts.EconStructures;

    public static class ShipManager
    {
        #region Market helpers

        public static void CreateSellOrder(long sellerId, ulong sellerSid, long shipId, decimal price)
        {
            var order = new ShipSaleStruct
            {
                Created = DateTime.Now,
                TraderId = sellerId,
                TraderSteamId = sellerSid,
                ShipId = shipId,
                Price = price,
                OptionalId = ""
            };
            EconomyScript.Instance.Data.ShipSale.Add(order);
        }

        public static decimal CheckSellOrder(long shipId)
        {
            var ship = EconomyScript.Instance.Data.ShipSale.FirstOrDefault(s => s.ShipId == shipId);

            if (ship == null)
				return 0;

            return ship.Price;
		}

        public static ulong GetOwner(long shipId)
        {
            var ship = EconomyScript.Instance.Data.ShipSale.FirstOrDefault(s => s.ShipId == shipId);

            if (ship == null)
                return 0;

			return ship.TraderSteamId;
		}

        public static decimal GetOwnerId(long shipId)
        {
            var ship = EconomyScript.Instance.Data.ShipSale.FirstOrDefault(s => s.ShipId == shipId);

            if (ship == null)
                return 0;

			return ship.TraderId;
		}

        public static bool Remove(long shipId, ulong sellerId)
        {
            var ship = EconomyScript.Instance.Data.ShipSale.FirstOrDefault(s => s.ShipId == shipId && s.TraderSteamId == sellerId);

            if (ship == null)
                return false;

			EconomyScript.Instance.Data.ShipSale.Remove(ship);
			return true;
		}

        #endregion
    }
}
