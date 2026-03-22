window.manufacturesform = window.manufacturesform || {};

window.manufacturesform.load = function (executionContext) {
    let formContext = executionContext.getFormContext();
    
    formContext.getAttribute("qzw_titleen").addOnChange(manufacturesform.titleen_onChange);
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

