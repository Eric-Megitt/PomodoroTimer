using System.Diagnostics;

static class PomodoroTimer {
	enum phase {
		Pomodoro,
		ShortBreak,
		LongBreak
	}
	
	static Dictionary<phase, string> phaseNameByPhase = new() {
		{ phase.Pomodoro, "Pomodoro" },
		{ phase.ShortBreak, "Short Break" },
		{ phase.LongBreak, "Long Break;" }
	};

	static Dictionary<phase, Func<TimeSpan>> durationByPhase = new() {
		{ phase.Pomodoro, () => pomodoroTime },
		{ phase.ShortBreak, () => shortBreakTime },
		{ phase.LongBreak, () => longBreakTime }
	};
	
	// variables
	static TimeSpan pomodoroTime = TimeSpan.FromMinutes(25);
	static TimeSpan shortBreakTime = TimeSpan.FromMinutes(5);
	static TimeSpan longBreakTime = TimeSpan.FromMinutes(15);
	static int pomodorosBeforeLongBreak = 4;
	
	// private variable
	static int pomodoroIndex = 0;
	
	// constants
	const string programName = nameof(PomodoroTimer);
	const string helpText = $"""
{programName}

[/StartPomodoro index]
[/S index]              - Decides which pomodoro you'll start on

[/TimeDurations m m m]
[/T m m m]              - Decides durations of pomodoro, short break & long break, respectively. Time provided in minutes

[/H]
[/?]                    - Brings up this screen. 

Press any key to continue...
""";
	
	// cli-arguments
	enum argument {
		StartPomodoro,
		TimeDurations,
		Help
	}
	static Dictionary<string, argument> argumentsByHandle = new() {
		{ "/S", argument.StartPomodoro },
		{ "/StartPomodoro", argument.StartPomodoro },
		
		{ "/T", argument.TimeDurations },
		{ "/TimeDurations", argument.TimeDurations },
		
		{ "/H", argument.Help },
		{ "/?", argument.Help }
	};
	static Dictionary<argument, int> parameterAmountByArgument = new() {
		{ argument.StartPomodoro, 1 },
		{ argument.TimeDurations, 3 },
		{ argument.Help, 0 }
	};
	
	static Dictionary<argument, Action<string[]>> methodByArgument = new() {
		{ argument.StartPomodoro, (s) => ChangeStartPomodoro(s) },
		{ argument.TimeDurations, (s) => ChangeTimeDurations(s) },
		{ argument.Help, (_) => Help() }
	};
	
	static async Task Main(string[] args) {
		ProcessArguments(args);

		await Timer();
	}

	static async Task Timer() {
		phase p;
		while (true) {
			if (pomodoroIndex % 2 == 0) {
				p = phase.Pomodoro;
			}
			else if (pomodoroIndex == (pomodorosBeforeLongBreak * 2) - 1) {
				p = phase.LongBreak;
			}
			else {
				p = phase.ShortBreak;
			}
			
			for (int m = durationByPhase[p].Invoke().Minutes; m > 0; m--) {
				Console.Clear();
				Console.WriteLine(phaseNameByPhase[p]);
				Console.WriteLine($"Minutes left: {m}");
				await Task.Delay(TimeSpan.FromMinutes(1));
			}

			pomodoroIndex++;
			pomodoroIndex %= pomodorosBeforeLongBreak * 2;
			Console.Beep();
		}
	}
	
	#region ArgumentHandling
	static void ProcessArguments(string[] args) {
		if (args.Length == 0) return;
		if (args[^1].Length == 0) {
			args = args.SkipLast(1).ToArray();
		}
		for (int i = 0; i < args.Length;) {
			if (!argumentsByHandle.ContainsKey(args[i])) {
				IncorrectArguments();
				return;
			}
			
			argument arg = argumentsByHandle[args[i]];
			i++;
			string[] p = args.Skip(i).SkipLast(args.Length - i - parameterAmountByArgument[arg]).ToArray();
			if (parameterAmountByArgument[arg] != 0) {
				ArraySegment<string> parameters;
				try {
					parameters = new(args, i, parameterAmountByArgument[arg]);
				}
				catch (Exception exception) {
					if (exception.GetType() != typeof(ArgumentException)) throw exception;

					IncorrectArguments();
					return;
				}


				if (p[^1].Length == 0) {
					p = p.SkipLast(1).ToArray();
				}

				if (p.Length != parameterAmountByArgument[arg]) {
					IncorrectArguments();
					return;
				}

				foreach (string s in p) {
					if (s[0] != '/') continue;

					IncorrectArguments();
					return;
				}

				Console.WriteLine($"length of p: {p.Length}");
			}

			methodByArgument[arg].Invoke(p);
			i += parameterAmountByArgument[arg];
		}
	}

	static void IncorrectArguments() {
		Debug.LogWarning($"Argument syntax was incorrect, for help write: \"{programName} /?\"");
		ReenterArguments();
	}
	
	static void ReenterArguments() {
		Console.WriteLine("Please enter program arguments again:");
		string[] arguments = (string.Empty + Console.ReadLine()).Split(" ");
		ProcessArguments(arguments);
	}
	#endregion ArgumentHandling
	
	#region ArgumentFunctionality
	static void ChangeStartPomodoro(string[] parameters) {
		int startPomodoro;
		try {
			startPomodoro = 2 * (int.Parse(parameters[0]) - 1);
		}
		catch (Exception exception) {
			if (exception.GetType() != typeof(FormatException)) throw exception;
			
			IncorrectArguments();
			return;
		}

		pomodoroIndex = startPomodoro;
	}
	
	static void ChangeTimeDurations(string[] parameters) {
		int pomodoro;
		int shortBreak;
		int longBreak;
		
		try {
			pomodoro = int.Parse(parameters[0]);
			shortBreak = int.Parse(parameters[1]);
			longBreak = int.Parse(parameters[2]);
		}
		catch (Exception exception) {
			if (exception.GetType() != typeof(FormatException)) throw exception;
			Debug.LogWarning("in catch");
			IncorrectArguments();
			return;
		}
		if (pomodoro <= 0 || shortBreak <= 0 || longBreak <= 0) {
			Debug.LogWarning("not in catch");
			IncorrectArguments();
			return;
		}
		
		pomodoroTime = TimeSpan.FromMinutes(pomodoro);
		shortBreakTime = TimeSpan.FromMinutes(shortBreak);
		longBreakTime = TimeSpan.FromMinutes(longBreak);
	}
	
	static void Help() {
		Console.Write(helpText);
		Console.ReadKey();
	}
	#endregion ArgumentFunctionality
}

static class Debug {
	static public void LogWarning(string warning) {
		ConsoleColor originalConsoleColour = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine(warning);
		Console.ForegroundColor = originalConsoleColour;
	}
}

/*
- async functions
- saved stats
- try catch
- inputs to quit program & skip phases & pause
*/
