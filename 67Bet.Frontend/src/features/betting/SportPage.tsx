import React from 'react';
import { useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { addSelection } from '../betslip/betslipSlice';
import type { RootState } from '../../app/store';
import { motion, AnimatePresence } from 'framer-motion';
import { Clock } from 'lucide-react';
import OddButton from './OddButton';

const MOCK_EVENTS = [
  { id: '1', name: 'Real Madrid vs Barcelona', league: 'La Liga', sport: 'Football', time: 'Today, 21:00', outcomes: [{ id: 'o1', name: '1', odd: 2.15 }, { id: 'o2', name: 'X', odd: 3.40 }, { id: 'o3', name: '2', odd: 3.10 }] },
  { id: '2', name: 'Man City vs Arsenal', league: 'Premier League', sport: 'Football', time: 'Today, 18:30', outcomes: [{ id: 'o4', name: '1', odd: 1.85 }, { id: 'o5', name: 'X', odd: 3.75 }, { id: 'o6', name: '2', odd: 4.20 }] },
  { id: '4', name: 'Lakers vs Celtics', league: 'NBA', sport: 'Basketball', time: 'Tomorrow, 02:00', outcomes: [{ id: 'o10', name: '1', odd: 1.90 }, { id: 'o11', name: 'X', odd: 12.00 }, { id: 'o12', name: '2', odd: 1.90 }] },
  { id: '5', name: 'G2 vs NAVI', league: 'PGL Major', sport: 'Esports', time: 'Today, 19:00', outcomes: [{ id: 'o13', name: '1', odd: 1.70 }, { id: 'o14', name: '2', odd: 2.10 }] },
];

const SportPage: React.FC = () => {
  const { sportName } = useParams<{ sportName: string }>();
  const dispatch = useDispatch();
  const selections = useSelector((state: RootState) => state.betslip.selections);

  const filteredEvents = MOCK_EVENTS.filter(e => 
    sportName === 'Popular' ? true : e.sport.toLowerCase() === sportName?.toLowerCase()
  );

  const isSelected = (outcomeId: string) => selections.some(s => s.outcomeId === outcomeId);

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-black text-white">{sportName}</h1>
        <span className="text-sm text-gray-500 font-bold uppercase">{filteredEvents.length} Events</span>
      </div>

      <div className="grid gap-4">
        {filteredEvents.length > 0 ? (
          filteredEvents.map((event) => (
            <motion.div 
              key={event.id}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-dark-800 border border-dark-700 rounded-2xl p-5 hover:border-dark-600 transition-colors"
            >
              <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex-1">
                  <div className="flex items-center gap-3 text-xs font-bold text-gray-500 mb-2 uppercase">
                    <span>{event.league}</span>
                    <div className="w-1 h-1 bg-dark-600 rounded-full" />
                    <div className="flex items-center gap-1">
                      <Clock className="w-3 h-3" />
                      {event.time}
                    </div>
                  </div>
                  <h3 className="text-lg font-bold text-white">{event.name}</h3>
                </div>

                <div className="flex items-center gap-2">
                  {event.outcomes.map((outcome) => (
                    <OddButton
                      key={outcome.id}
                      name={outcome.name}
                      odd={outcome.odd}
                      isSelected={isSelected(outcome.id)}
                      onClick={() => dispatch(addSelection({
                        eventId: event.id,
                        eventName: event.name,
                        marketId: 'm1',
                        marketName: 'Match Result',
                        outcomeId: outcome.id,
                        outcomeName: outcome.name === '1' ? event.name.split(' vs ')[0] : outcome.name === '2' ? event.name.split(' vs ')[1] : 'Draw',
                        odd: outcome.odd
                      }))}
                    />
                  ))}
                </div>
              </div>
            </motion.div>
          ))
        ) : (
          <div className="h-64 flex flex-col items-center justify-center text-gray-500 bg-dark-800 rounded-3xl border border-dark-700 border-dashed">
            <p className="font-bold">No active events for {sportName}</p>
            <p className="text-xs">Check back later or try AI Custom Bet!</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default SportPage;
