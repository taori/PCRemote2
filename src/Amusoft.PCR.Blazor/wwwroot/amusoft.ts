namespace Amusoft {
	export class Page {
		public static setTitle(title: string) {
			document.title = title;
		}
	}

	export class Functions {

		public static Log(message: string) {
//			console.log(message);
		}

		public static showPrompt(message: string, watermark: string) : string {
			return prompt(message, watermark);
		}

		public static alert(message: string) : void {
			alert(message);
		}

		public static disable(element: any): void {
			element.setAttribute("disabled", true);
			this.Log("Disable");
		}

		public static enable(element: any): void {
			element.removeAttribute("disabled");
			this.Log("Enable");
		}
	}
}

namespace Amusoft.Components {
	export class ModalDialog {
		public static initialize(element: HTMLElement): void {
			element.style.setProperty("background-color", "red");
			document.querySelectorAll("app")[0].appendChild(element);
//			document.body.style.setProperty("position", "relative");
		}
	}
}