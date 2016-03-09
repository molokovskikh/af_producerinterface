bindJsonToModel = function (JsonModel, Type) {
    /* первая модель получаемая от сервера при старте страницы */
    if (Type == 0) {
        bindDataFeedBack(JsonModel);
        bindDataFilter(JsonModel);
    }
    /* модель полученная по фильтру (Она таже самая, но фильтр не вернулся к нам второй раз, он у нас уже есть) */
    if (Type == 1) {
        bindDataFeedBack(JsonModel);
    }
}

bindDataFeedBackOne = function (FeedOneModel) {
    FeedBackOneModel.FeedBackOneId(FeedOneModel.Id);
    FeedBackOneModel.FeedBackOneDescription(FeedOneModel.Description);
    FeedBackOneModel.FeedBackDate(FeedOneModel.DateTimeString);
    FeedBackOneModel.FeedbackType(FeedOneModel.TypeString);
    FeedBackOneModel.FeedBackContact(FeedOneModel.About);

    FeedBackOneModel.FeedBackOneId.valueHasMutated();
    FeedBackOneModel.FeedBackOneDescription.valueHasMutated();
    FeedBackOneModel.FeedBackDate.valueHasMutated();
    FeedBackOneModel.FeedbackType.valueHasMutated();
    FeedBackOneModel.FeedBackContact.valueHasMutated();
}

/* првязка данных к фильтру */
bindDataFilter = function (ModelResult) {
    var ModelFeedBackFilter = ModelResult.FeedBackFilter;

    var beginDate = ModelFeedBackFilter.DateBegin;
    var endDate = ModelFeedBackFilter.DateEnd;

    /* фильтр модель выбора количества страниц */
    FilterModel.PagerCountList(ModelResult.PageCount);
    FilterModel.PagerCountList.valueHasMutated();

    FilterModel.DateBegin(beginDate);
    FilterModel.DateEnd(endDate);
    FilterModel.LoginId(ModelFeedBackFilter.LoginId);
    FilterModel.Pager(ModelFeedBackFilter.Pager);
    FilterModel.ProducerId(ModelFeedBackFilter.ProducerId);
    FilterModel.LoginId(ModelFeedBackFilter.LoginId);

    FilterModel.DateBegin.valueHasMutated();
    FilterModel.DateEnd.valueHasMutated();
    FilterModel.LoginId.valueHasMutated();
    FilterModel.Pager.valueHasMutated();
    FilterModel.ProducerId.valueHasMutated();
    FilterModel.LoginId.valueHasMutated();

    /* список производителей для фильтра */
    FilterModel.ProducerList(ModelResult.ProducerList);
    FilterModel.ProducerList.valueHasMutated();

    /* список пользователей для фильтра */
    FilterModel.AccountList(ModelResult.AccountList);
    FilterModel.AccountList.valueHasMutated();

    /* список для фильтрации количества отображаемых строк на странице пайджера */
    FilterModel.PagerCountList(ModelResult.PageCount);
    FilterModel.PagerCountList.valueHasMutated();
}

    /* привязка данных к модули отображения запросов пользователей и обновление пагинатора */
    bindDataFeedBack = function (ModelResult) {

        /* список обращений */
        FeedBackListModel.Feedback(ModelResult.FeedBackList);
        FeedBackListModel.Feedback.valueHasMutated();

        /* добаление css  для отображения по каким столбца произведена фильтрация*/
        FeedBackListModel.SortAccountName(ModelResult.SortAccountName);
        FeedBackListModel.SortAccountName.valueHasMutated();
        FeedBackListModel.SortProducerName(ModelResult.SortProducerName);
        FeedBackListModel.SortProducerName.valueHasMutated();
        FeedBackListModel.SortStatus(ModelResult.SortStatus);
        FeedBackListModel.SortStatus.valueHasMutated();
        FeedBackListModel.SortTime(ModelResult.SortTime);
        FeedBackListModel.SortTime.valueHasMutated();
        FeedBackListModel.SortType(ModelResult.SortType);
        FeedBackListModel.SortType.valueHasMutated();

        /* максимальное количество страниц */
        FeedBackListModel.MaxPageCount(ModelResult.MaxPageCount);
        FeedBackListModel.MaxPageCount.valueHasMutated();

        /* Текущая страница пагинатора */
        FilterModel.PagerIndex(ModelResult.PagerIndex);
        FilterModel.PagerIndex.valueHasMutated();

        /* Текущая страница пагинатора */
        FeedBackListModel.PagerIndex(ModelResult.PagerIndex);
        FeedBackListModel.PagerIndex.valueHasMutated();

        /* Модель пагинатора */
        FilterModel.PaginatorLinks(ModelResult.PaginatorLinks);
        FilterModel.PaginatorLinks.valueHasMutated();
    }
