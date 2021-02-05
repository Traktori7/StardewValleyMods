using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace VersatileGrabber
{
	public class VersatileGrabber : SObject
	{
		public readonly List<Item> items = new List<Item>();

		public VersatileGrabber() : base ()
		{

		}


		public VersatileGrabber(SObject sobject) : base (sobject.TileLocation, sobject.ParentSheetIndex)
		{
			
		}


		public void AddItemToInventory(Item item)
		{
			items.Add(item);
		}


		/// <summary>
		/// When users picks something from their inventory.
		/// TODO: Just a temporary solution
		/// </summary>
		/// <param name="item"></param>
		/// <param name="who"></param>
		public void BehaviourOnItemSelect(Item item, Farmer who)
		{
			if (item.Stack == 0)
				item.Stack = 1;

			bool itemWasAddedToList = false;

			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null && items[i].canStackWith(item))
				{
					item.Stack = items[i].addToStack(item);
					if (item.Stack <= 0)
						itemWasAddedToList = true;
				}
			}

			if (!itemWasAddedToList)
			{
				if (items.Count < 36)
				{
					items.Add(item);
					itemWasAddedToList = true;
				}
			}

			if (itemWasAddedToList)
				who.removeItemFromInventory(item);

			int id = Game1.activeClickableMenu.currentlySnappedComponent != null ? Game1.activeClickableMenu.currentlySnappedComponent.myID : -1;

			Game1.activeClickableMenu = new ItemGrabMenu(items, false, true, InventoryMenu.highlightAllItems,
				(Item temp, Farmer farmer) => BehaviourOnItemSelect(temp, farmer),
				null,
				(Item temp, Farmer farmer) => BehaviourOnItemGrab(temp, farmer),
				false, true, true, true, true, 0, null, -1, null);

			(Game1.activeClickableMenu as ItemGrabMenu).heldItem = item;
			if (id == -1)
				return;
			Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(id);
			Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
		}


		/// <summary>
		/// When users picks something from the machine's inventory.
		/// TODO: Just a temporary solution
		/// </summary>
		/// <param name="item"></param>
		/// <param name="who"></param>
		public void BehaviourOnItemGrab(Item item, Farmer who)
		{
			if (!who.couldInventoryAcceptThisItem(item))
				return;
			this.items.Remove(item);
			this.items.RemoveAll(listItem => listItem == null);

			Game1.activeClickableMenu = new ItemGrabMenu(items, false, true, InventoryMenu.highlightAllItems,
				(Item temp, Farmer farmer) => BehaviourOnItemSelect(temp, farmer),
				null,
				(Item temp, Farmer farmer) => BehaviourOnItemGrab(temp, farmer),
				false, true, true, true, true, 0, null, -1, null);
		}


		public SObject ToObject()
		{
			SObject newObject = new SObject(this.TileLocation, this.ParentSheetIndex);
			return newObject;
		}


		/*************/
		/* Overrides */
		/*************/
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			// I don't know what this flag does...
			if (justCheckingForActivity)
				return true;

			//Game1.activeClickableMenu = new GrabberItemGrabMenu((IList<Item>)(this.heldObject.Value as Chest).items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect((this.heldObject.Value as Chest).grabItemFromInventory), (string)null, new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromAutoGrabber), false, true, true, true, true, 1, (Item)null, -1, (object)this);
			//Game1.activeClickableMenu = new GrabberItemGrabMenu(items);
			Game1.activeClickableMenu = new ItemGrabMenu(items, false, true, InventoryMenu.highlightAllItems,
				(Item item, Farmer farmer) => BehaviourOnItemSelect(item, farmer),
				null,
				(Item item, Farmer farmer) => BehaviourOnItemGrab(item, farmer),
				false, true, true, true, true, 0, null, -1, null);

			return true;
		}


		/// <summary>
		/// Draws the object every draw tick. Used to control the appearance of the machine.
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="alpha"></param>
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
		{
			// The scaling vector, used possibly for the working animation
			Vector2 vector2 = this.getScale() * 4f;
			// The tiles position in some coordinates?
			Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			// The rectangle on screen where the object will be drawn
			Rectangle destinationRectangle = new Rectangle((int)(local.X - vector2.X / 2.0) + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(local.Y - vector2.Y / 2.0) + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(64.0 + vector2.X), (int)(128.0 + vector2.Y / 2.0));
			// The layer where the object will be drawn
			float layerDepth = Math.Max(0.0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f;
			//spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, new Microsoft.Xna.Framework.Rectangle?(Object.getSourceRectForBigCraftable((bool)(NetFieldBase<bool, NetBool>)this.showNextIndex ? this.ParentSheetIndex + 1 : this.ParentSheetIndex)), Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, layerDepth);

			// Draw the whole texture for now
			spriteBatch.Draw(ModEntry.texture, destinationRectangle, new Rectangle(0, 0, 16, 32), Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, layerDepth);
		}


		/// <summary>
		/// You can't put anything straight to the object, interactions will be handled with checkForAction which opens the inner menu
		/// </summary>
		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
		{
			return false;
		}


		
	}
}
