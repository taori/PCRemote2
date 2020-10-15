var Amusoft;
(function (Amusoft) {
    var Functions = /** @class */ (function () {
        function Functions() {
        }
        Functions.showPrompt = function (message, watermark) {
            return prompt(message, watermark);
        };
        Functions.alert = function (message) {
            alert(message);
        };
        return Functions;
    }());
    Amusoft.Functions = Functions;
})(Amusoft || (Amusoft = {}));
//# sourceMappingURL=amusoft.js.map