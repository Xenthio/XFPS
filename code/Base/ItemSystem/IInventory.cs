namespace XBase;

/// <summary>
/// The IInventory interface is the base interface for all inventories.
/// It defines the basic functions that all inventory systems must implement.
/// </summary>
public interface IInventory
{
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
	/// Removes an item from the inventory.
	/// </summary>
	/// <param name="item">The item to remove.</param>
	/// <returns>True if the item was removed successfully, false otherwise.</returns>
	bool RemoveItem( BaseCarryable item );
}
