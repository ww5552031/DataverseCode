window.manufacturesform = window.manufacturesform || {};

// 窗体状态常量（映射 Xrm.FormType 值）
window.manufacturesform.FormType = {
    Undefined: 0,
    Create: 1,
    Update: 2,
    ReadOnly: 3,
    Disabled: 4,
    QuickCreate: 5,
    BulkEdit: 6
};

window.manufacturesform.load = function (executionContext) {
    let formContext = executionContext.getFormContext();
    //获取窗体状态
    let formType = formContext.ui.getFormType();
    
    debugger;
    if (formType != manufacturesform.FormType.ReadOnly && 
        formType != manufacturesform.FormType.Disabled) {


        formContext.getAttribute("qzw_titleen").addOnChange(manufacturesform.titleen_onChange);
    }
}


window.manufacturesform.titleen_onChange = function (executionContext) {

    debugger;
    let formContext = executionContext.getFormContext();
    let eventSource = executionContext.getEventSource();
    let fieldVal = eventSource.getValue();
    let fieldControl = eventSource.controls.get(0);
    if (fieldVal) {
        fieldControl.clearNotification("phoneWarning");
    } else {
        fieldControl.setNotification("哈哈哈", "phoneWarning");
    }

}

