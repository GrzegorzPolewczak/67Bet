import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { bettingApi, oddsApi } from "../../api/axios";

export type EventSource = "internal" | "external";

export interface Outcome {
  id: string;
  name: string;
  odd: number;
  isBettable: boolean;
}

export interface Market {
  id: string;
  name: string;
  outcomes: Outcome[];
}

export interface Event {
  id: string;
  name: string;
  league: string;
  sportKey: string;
  time: string;
  rawTime: string;
  markets: Market[];
  source: EventSource;
  isBettable: boolean;
}

interface BettingState {
  events: Event[];
  loading: boolean;
  error: string | null;
}

type ApiObject = Record<string, unknown>;

const asObject = (value: unknown): ApiObject =>
  typeof value === "object" && value !== null ? (value as ApiObject) : {};

const asArray = (value: unknown): unknown[] =>
  Array.isArray(value) ? value : [];

const initialState: BettingState = {
  events: [],
  loading: false,
  error: null,
};

const MOCK_EVENTS: Event[] = [
  {
    id: "00000000-0000-0000-0000-000000000001",
    name: "Real Madrid vs Bayern Munich",
    league: "Champions League",
    sportKey: "soccer",
    time: "20:45",
    rawTime: new Date().toISOString(),
    source: "internal",
    isBettable: true,
    markets: [
      {
        id: "11111111-1111-1111-1111-111111111111",
        name: "Match Winner",
        outcomes: [
          {
            id: "a0000000-0000-0000-0000-000000000001",
            name: "1",
            odd: 2.1,
            isBettable: true,
          },
          {
            id: "a0000000-0000-0000-0000-000000000002",
            name: "X",
            odd: 3.5,
            isBettable: true,
          },
          {
            id: "a0000000-0000-0000-0000-000000000003",
            name: "2",
            odd: 3.2,
            isBettable: true,
          },
        ],
      },
    ],
  },
  {
    id: "00000000-0000-0000-0000-000000000002",
    name: "FC Barcelona vs Juventus",
    league: "Champions League",
    sportKey: "soccer",
    time: "21:00",
    rawTime: new Date().toISOString(),
    source: "internal",
    isBettable: true,
    markets: [
      {
        id: "22222222-2222-2222-2222-222222222222",
        name: "Match Winner",
        outcomes: [
          {
            id: "b0000000-0000-0000-0000-000000000001",
            name: "1",
            odd: 1.85,
            isBettable: true,
          },
          {
            id: "b0000000-0000-0000-0000-000000000002",
            name: "X",
            odd: 3.8,
            isBettable: true,
          },
          {
            id: "b0000000-0000-0000-0000-000000000003",
            name: "2",
            odd: 4.1,
            isBettable: true,
          },
        ],
      },
    ],
  },
];

const toSafeString = (value: unknown, fallback = ""): string => {
  if (typeof value === "string" && value.trim()) return value.trim();
  if (typeof value === "number") return String(value);
  return fallback;
};

const toOdd = (value: unknown): number => {
  const parsed = Number(value);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : 0;
};

const formatDateTime = (value: unknown): string => {
  const date = new Date(toSafeString(value));
  if (Number.isNaN(date.getTime())) return "Scheduled";

  return date.toLocaleString("pl-PL", {
    day: "2-digit",
    month: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
};

const getInternalOutcomeName = (eventName: string, outcomeName: string): string => {
  const [homeTeam, awayTeam] = eventName.split(" vs ");
  if (outcomeName === "1") return homeTeam || "Team 1";
  if (outcomeName === "2") return awayTeam || "Team 2";
  return outcomeName;
};

const marketName = (name: string): string => {
  if (name === "h2h") return "Match Winner";
  if (name === "spreads") return "Spread";
  if (name === "totals") return "Totals";
  return name || "Market";
};

const mapInternalEvent = (eventRaw: unknown): Event => {
  const event = asObject(eventRaw);
  const eventName = toSafeString(event.name, "Unknown Event");
  const rawTime = toSafeString(event.startTime ?? event.rawTime);

  return {
    id: toSafeString(event.id),
    name: eventName,
    league: toSafeString(event.league, "67Bet"),
    sportKey: toSafeString(event.sportKey, "internal"),
    rawTime,
    time: formatDateTime(rawTime),
    source: "internal",
    isBettable: true,
    markets: asArray(event.markets).map((marketRaw) => {
      const market = asObject(marketRaw);

      return {
        id: toSafeString(market.id),
        name: marketName(toSafeString(market.name, "Match Winner")),
        outcomes: asArray(market.outcomes).map((outcomeRaw) => {
          const outcome = asObject(outcomeRaw);
          const outcomeName = toSafeString(outcome.name, "-");

          return {
            id: toSafeString(outcome.id),
            name: getInternalOutcomeName(eventName, outcomeName),
            odd: toOdd(outcome.currentPrice ?? outcome.odd ?? outcome.price),
            isBettable: Boolean(outcome.id),
          };
        }),
      };
    }),
  };
};

const mapExternalEvent = (eventRaw: unknown): Event => {
  const event = asObject(eventRaw);
  const externalId = toSafeString(event.id ?? event.externalId, "unknown-external-event");
  const homeTeam = toSafeString(event.homeTeam ?? event.home_team, "Home");
  const awayTeam = toSafeString(event.awayTeam ?? event.away_team, "Away");
  const eventName = `${homeTeam} vs ${awayTeam}`;
  const rawTime = toSafeString(
    event.commenceTime ?? event.commence_time ?? event.startTime,
  );
  const primaryBookmaker = asObject(asArray(event.bookmakers)[0]);

  return {
    id: `external:${externalId}`,
    name: eventName,
    league: toSafeString(event.sportTitle ?? event.sport_title, "External Odds"),
    sportKey: toSafeString(event.sportKey ?? event.sport_key, "external"),
    rawTime,
    time: formatDateTime(rawTime),
    source: "external",
    isBettable: false,
    markets: asArray(primaryBookmaker.markets).map((marketRaw, marketIndex) => {
      const market = asObject(marketRaw);
      const key = toSafeString(market.key, `market-${marketIndex}`);

      return {
        id: `external:${externalId}:${key}`,
        name: marketName(key),
        outcomes: asArray(market.outcomes).map((outcomeRaw, outcomeIndex) => {
          const outcome = asObject(outcomeRaw);
          const outcomeName = toSafeString(outcome.name, "outcome");

          return {
            id: `external:${externalId}:${key}:${outcomeIndex}:${outcomeName}`,
            name: toSafeString(outcome.name, "-"),
            odd: toOdd(outcome.price ?? outcome.odd ?? outcome.currentPrice),
            isBettable: false,
          };
        }),
      };
    }),
  };
};

const fetchExternalEvents = async (): Promise<unknown[]> => {
  const response = await oddsApi.get("/ExternalOdds/events");
  const events = Array.isArray(response.data) ? response.data : [];

  if (events.length > 0) return events;

  try {
    await oddsApi.post("/ExternalOdds/sync");
    const syncedResponse = await oddsApi.get("/ExternalOdds/events");
    return Array.isArray(syncedResponse.data) ? syncedResponse.data : [];
  } catch {
    return events;
  }
};


const toDateMs = (value: string): number => {
  const time = new Date(value).getTime();
  return Number.isNaN(time) ? Number.MAX_SAFE_INTEGER : time;
};

const uniqueBySourceAndId = (events: Event[]): Event[] => {
  const seen = new Set<string>();
  return events.filter((event) => {
    const key = `${event.source}:${event.id}`;
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
};

export const fetchEventsAsync = createAsyncThunk(
  "betting/fetchEvents",
  async (_, { rejectWithValue }) => {
    const errors: string[] = [];
    const [internalResult, externalResult] = await Promise.allSettled([
      bettingApi.get("/events"),
      fetchExternalEvents(),
    ]);

    const events: Event[] = [];

    if (internalResult.status === "fulfilled") {
      const data = Array.isArray(internalResult.value.data)
        ? internalResult.value.data
        : [];
      events.push(...data.map(mapInternalEvent));
    } else {
      errors.push("Betting API unavailable");
    }

    if (externalResult.status === "fulfilled") {
      events.push(...externalResult.value.map(mapExternalEvent));
    } else {
      errors.push("External Odds API unavailable");
    }

    if (events.length === 0 && errors.length > 0) {
      return rejectWithValue(errors.join("; "));
    }

    return uniqueBySourceAndId(events).sort(
      (a, b) => toDateMs(a.rawTime) - toDateMs(b.rawTime),
    );
  },
);

const bettingSlice = createSlice({
  name: "betting",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchEventsAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchEventsAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.events = action.payload;
      })
      .addCase(fetchEventsAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
        state.events = MOCK_EVENTS;
      });
  },
});

export default bettingSlice.reducer;
