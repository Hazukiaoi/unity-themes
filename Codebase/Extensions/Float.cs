public static class FloatExtension{
	public static float RoundClosestDown(this float current,params float[] values){
		float highest = -1;
		foreach(float value in values){
			if(current >= value){
				highest = value;
				break;
			}
		}
		foreach(float value in values){
			if(current >= value && value > highest){
				highest = value;
			}
		}
		return highest;
	}
	public static float RoundClosestUp(this float current,params float[] values){
		float lowest = -1;
		foreach(float value in values){
			if(current >= value){
				lowest = value;
				break;
			}
		}
		foreach(float value in values){
			if(current <= value && value < lowest){
				lowest = value;
			}
		}
		return lowest;
	}
}