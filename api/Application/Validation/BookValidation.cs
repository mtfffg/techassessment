using Domain.Enums;

namespace Application.Validation;

public static class BookValidation
{
    public static void ValidateStoreDetails(EGenre? genre, decimal? price, int? stock)
    {
        //check genre and price
        if (!genre.HasValue || !Enum.IsDefined(genre.Value))
        {
            throw new ArgumentException("Select a genre.");
        }
        if (!price.HasValue || price < 0 || price > 99999999.99m || decimal.Round(price.Value, 2) != price.Value)
        {
            throw new ArgumentException("Enter a price between 0 and 99999999.99 with up to two decimal places.");
        }
        if (!stock.HasValue || stock < 0)
        {
            throw new ArgumentException("Enter a stock quantity of zero or more.");
        }
    }
}
