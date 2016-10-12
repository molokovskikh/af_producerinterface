modalPicture = function() {
	var _this = this;

	this.nameHref = "a[name='pictureUploadUrl']";
	this.nameImg = "img[name='pictureUploadUrl']";
	this.nameCatalogId = "input[id='PictureUploadCatalogId']";
	this.nameProducerId = "input[id='PictureUploadProducerId']";
	this.nameDrugFamilyId = "input[id='DrugFamilyId']";

	this.attrImgUrl = "iUrl";
	this.attrCatalogId = "cId";
	this.attrProducerId = "pId";
	this.attrDrugFamilyId = "dId";


	this.SetModelData = function(item) {
		var imgUrl = $(item).attr(_this.attrImgUrl),
			catalogId = $(item).attr(_this.attrCatalogId),
			producerId = $(item).attr(_this.attrProducerId),
			drugFamilyId = $(item).attr(_this.attrDrugFamilyId);

		$(_this.nameHref).attr('href', imgUrl);
		$(_this.nameImg).attr('src', imgUrl);
		$(_this.nameCatalogId).val(catalogId);
		$(_this.nameProducerId).val(producerId);
		$(_this.nameDrugFamilyId).val(drugFamilyId);
	}
}