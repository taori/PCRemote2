namespace Amusoft {
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