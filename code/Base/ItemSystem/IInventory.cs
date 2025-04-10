namespace XBase;

/// <summary>
/// The IInventory interface is the base interface for all inventories.
/// It defines the basic functions that all inventory systems must implement.
/// </summary>
public interface IInventory
{
	/// <summary>
	/// Gets a list of all items in the inventory.
	/// </summary>
	/// <param name="item">The item to remove.</param>
	/// <returns>True if the item was removed successfully, false otherwise.</returns>
	List<BaseCarryable> GetAllItems();

	/// <summary>
	/// Sets the current item in the inventory.
	/// </summary>
	/// <param name="item">The item to set as current.</param>
	void SetCurrentItem( BaseCarryable item );

	/// <summary>
	/// Gets the current item in the inventory.
	/// </summary>
	/// <returns>The current item.</returns>
	BaseCarryable GetCurrentItem();

	/// <summary>
	/// Adds an item to the inventory.
	/// </summary>
	/// <param name="item">The item to add.</param>
	/// <returns>True if the item was added successfully, false otherwise.</returns>
	bool AddItem( BaseCarryable item );

	/// <summary>
	/// Adds an item to the inventory.
	/// </summary>
	/// <param name="prefab">The prefab containing the item to add.</param>
	/// <returns>True if the item was added successfully, false otherwise.</returns>
	bool AddItemFromPrefab( GameObject prefab );

	/// <summary>
	/// Removes an item from the inventory.
	/// </summary>
	/// <param name="item">The item to remove.</param>
	/// <returns>True if the item was removed successfully, false otherwise.</returns>
	bool RemoveItem( BaseCarryable item );
}
