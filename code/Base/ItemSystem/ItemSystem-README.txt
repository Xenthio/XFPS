# The Item System

The item system allows for gameobjects (Players and NPCs) to carry items, its designed to work with an implementation of the inventory system.

# --- Items ---

Items are split into 2 types
- Carriables: The base item type. These are the items that can be carried by the player.
- Weapons: A subclass of Equippables. These are items that can be used by the player.

### Carriables:
Carriables are the base item type. These are items that can be held out by the player. They must have a worldmodel (and a viewmodel, if applicable to your game).

### Weapons: 
Weapons are a subclass of Equippables. These are items that can be used by the player. They must have a worldmodel (and a viewmodel, if applicable to your game).

# --- Inventories ---

Inventories can vary in size and type, you can store items however you want (examples, Half-Life styled buckets to Minecraft styled slots (with stacking even)), as long as you can fetch the selected item and change the selected item.
There is a built in bucket style inventory system, which is a simple array of items. You can also create your own inventory system by implementing the IInventory interface.

### IInventory:
The IInventory interface is the base interface for all inventories. It defines the basic functions that all inventory systems must implement.

- SetCurrentItem: Sets the current item in the inventory.
- GetCurrentItem: Gets the current item in the inventory.
- AddItem: Adds an item to the inventory.
- RemoveItem: Removes an item from the inventory.


