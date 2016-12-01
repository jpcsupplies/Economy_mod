namespace Economy.scripts.EconConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Economy.scripts.EconStructures;
    using Messages;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRageMath;
    using IMyTerminalBlock = Sandbox.ModAPI.Ingame.IMyTerminalBlock;
    using IMyBeacon = Sandbox.ModAPI.Ingame.IMyBeacon;

    public static class ShipManager
    {
        #region Market helpers

        public static void CreateSellOrder(decimal sellerId, ulong sellerSid, string ShipID, decimal price)
        {
            var order = new ShipSaleStruct
            {
                Created = DateTime.Now,
                TraderId = sellerId,
                TraderSid = sellerSid,
                ShipID = ShipID,
                Price = price,
                OptionalId = ""
            };
            EconomyScript.Instance.Data.ShipSale.Add(order);
        }

        public static decimal CheckSellOrder(string ShipID)
        {
            var ships = EconomyScript.Instance.Data.ShipSale.Where(s => s.ShipID == ShipID).ToArray();

            if (ships.Length == 0)
            {
				return 0;
            }

            foreach (var ship in ships)
            {
				return ship.Price;
            }
			return 0;
		}

        public static ulong GetOwner(string ShipID)
        {
            var ships = EconomyScript.Instance.Data.ShipSale.Where(s => s.ShipID == ShipID).ToArray();

            if (ships.Length == 0)
            {
				return 0;
            }

            foreach (var ship in ships)
            {
				return ship.TraderSid;
            }
			return 0;
		}

        public static decimal GetOwnerID(string ShipID)
        {
            var ships = EconomyScript.Instance.Data.ShipSale.Where(s => s.ShipID == ShipID).ToArray();

            if (ships.Length == 0)
            {
				return 0;
            }

            foreach (var ship in ships)
            {
				return ship.TraderId;
            }
			return 0;
		}

        public static bool Remove(string ShipID, ulong sellerId)
        {
            var ships = EconomyScript.Instance.Data.ShipSale.Where(s => s.ShipID == ShipID).ToArray();

            if (ships.Length == 0)
            {
				return false;
            }

            foreach (var ship in ships)
            {
				if(ship.TraderSid == sellerId)
				{
					EconomyScript.Instance.Data.ShipSale.Remove(ship);
					return true;
				}
            }
			return false;
		}

        #endregion
    }
}
