-- AboutContents tablosundaki NULL değerleri temizleme scripti
-- Bu script, veritabanındaki NULL string değerlerini boş string ile değiştirir

UPDATE AboutContents 
SET 
    Title = ISNULL(Title, ''),
    Subtitle = ISNULL(Subtitle, ''),
    Content = ISNULL(Content, ''),
    ImageUrl = ISNULL(ImageUrl, ''),
    StoryTitle = ISNULL(StoryTitle, ''),
    StorySubtitle = ISNULL(StorySubtitle, ''),
    StoryContent = ISNULL(StoryContent, ''),
    StoryImageUrl = ISNULL(StoryImageUrl, ''),
    MissionTitle = ISNULL(MissionTitle, ''),
    MissionContent = ISNULL(MissionContent, ''),
    VisionTitle = ISNULL(VisionTitle, ''),
    VisionContent = ISNULL(VisionContent, ''),
    ValuesTitle = ISNULL(ValuesTitle, ''),
    ValuesSubtitle = ISNULL(ValuesSubtitle, ''),
    ValuesContent = ISNULL(ValuesContent, ''),
    ValueItems = ISNULL(ValueItems, '[]'),
    ProductionTitle = ISNULL(ProductionTitle, ''),
    ProductionSubtitle = ISNULL(ProductionSubtitle, ''),
    ProductionSteps = ISNULL(ProductionSteps, '[]'),
    CertificatesTitle = ISNULL(CertificatesTitle, ''),
    CertificatesSubtitle = ISNULL(CertificatesSubtitle, ''),
    CertificateItems = ISNULL(CertificateItems, '[]'),
    RegionTitle = ISNULL(RegionTitle, ''),
    RegionSubtitle = ISNULL(RegionSubtitle, ''),
    RegionContent = ISNULL(RegionContent, ''),
    RegionImageUrl = ISNULL(RegionImageUrl, ''),
    RegionFeatures = ISNULL(RegionFeatures, '[]'),
    CtaTitle = ISNULL(CtaTitle, ''),
    CtaContent = ISNULL(CtaContent, ''),
    CtaButtonText = ISNULL(CtaButtonText, ''),
    CtaSecondButtonText = ISNULL(CtaSecondButtonText, ''),
    StoryFeatures = ISNULL(StoryFeatures, '[]')
WHERE IsActive = 1;

PRINT 'AboutContents tablosundaki NULL değerler temizlendi!';
