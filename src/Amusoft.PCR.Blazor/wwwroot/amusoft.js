var Amusoft;
(function (Amusoft) {
    var Functions = /** @class */ (function () {
        function Functions() {
        }
        Functions.Log = function (message) {
            console.log(message);
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
//# sourceMappingURL=amusoft.js.map