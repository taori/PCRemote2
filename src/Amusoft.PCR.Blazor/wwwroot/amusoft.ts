namespace Amusoft {
	export class Functions {
		public static showPrompt(message: string, watermark: string) : string {
			return prompt(message, watermark);
		}

		public static alert(message: string) : void {
			alert(message);
		}
	}
}