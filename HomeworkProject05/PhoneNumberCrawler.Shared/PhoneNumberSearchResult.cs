namespace PhoneNumberCrawler.Shared;

public record class PhoneNumberSearchResult(string PhoneNumber, HashSet<Uri> Sources);
