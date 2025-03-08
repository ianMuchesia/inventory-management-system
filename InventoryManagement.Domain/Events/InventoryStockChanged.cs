using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Domain.Events
{
    public class InventoryStockChanged
    {
        public int ProductId { get; }
        public int NewQuantity { get; }
        public TransactionType TransactionType { get; }
        public int QuantityChanged { get; }
        public DateTime TimeStamp { get; }

        public InventoryStockChanged(int productId, int newQuantity, TransactionType transactionType, int quantityChanged)
        {
            ProductId = productId;
            NewQuantity = newQuantity;
            TransactionType = transactionType;
            QuantityChanged = quantityChanged;
            TimeStamp = DateTime.UtcNow;
        }
    }
}