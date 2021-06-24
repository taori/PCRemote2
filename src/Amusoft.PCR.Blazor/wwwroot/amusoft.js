var Amusoft;
(function (Amusoft) {
    var Page = /** @class */ (function () {
        function Page() {
        }
        Page.setTitle = function (title) {
            document.title = title;
        };
        return Page;
    }());
    Amusoft.Page = Page;
    var Functions = /** @class */ (function () {
        function Functions() {
        }
        Functions.Log = function (message) {
            //			console.log(message);
        };
        Functions.showPrompt = function (message, watermark) {
            return prompt(message, watermark);
        };
        Functions.alert = function (message) {
            alert(message);
        };
        Functions.disable = function (element) {
            element.setAttribute("disabled", true);
            this.Log("Disable");
        };
        Functions.enable = function (element) {
            element.removeAttribute("disabled");
            this.Log("Enable");
        };
        return Functions;
    }());
    Amusoft.Functions = Functions;
})(Amusoft || (Amusoft = {}));
(function (Amusoft) {
    var Components;
    (function (Components) {
        var ModalDialog = /** @class */ (function () {
            function ModalDialog() {
            }
            ModalDialog.initialize = function (element) {
                element.classList.add("amu-modal-wrapper");
                document.querySelectorAll("app")[0].appendChild(element);
            };
            return ModalDialog;
        }());
        Components.ModalDialog = ModalDialog;
    })(Components = Amusoft.Components || (Amusoft.Components = {}));
})(Amusoft || (Amusoft = {}));
//# sourceMappingURL=amusoft.js.map