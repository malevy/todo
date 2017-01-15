﻿using System;
using System.Collections.Generic;

namespace D2L.Hypermedia.Siren {

	public interface ISirenAction {

		string Name { get; }

		string[] Class { get; }

		string Method { get; }

		Uri Href { get; }

		string Title { get; }

		string Type { get; }

		IEnumerable<ISirenField> Fields { get; }

	}

}