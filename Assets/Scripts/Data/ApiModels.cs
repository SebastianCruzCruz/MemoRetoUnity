using System;
using System.Collections.Generic;

namespace MemoRetos.Data
{
    [Serializable] public class LoginRequest
    {
        public string username;
        public string password;
    }

    [Serializable] public class LoginResponse
    {
        public string   token;
        public UserData user;
    }

    [Serializable] public class UserData
    {
        public int    id;
        public string name;
        public string lastname;
        public string username;
        public string email;
        public string rol;
        public string group;
        public int    total_score;
        public bool   tutorial_completed;
    }

    [Serializable] public class FiguraData
    {
        public string       type;
        public List<string> nodos;
    }

    [Serializable] public class InterseccionData
    {
        public string       nodo;
        public List<string> figuras;
    }

    [Serializable] public class MemoRetoData
    {
        public int                    id;
        public string                 title;
        public int                    nivel;
        public string                 dificultad;
        public bool                   is_published;
        public List<int>              number_set;
        public List<FiguraData>       figuras;
        public List<InterseccionData> intersecciones;
    }

    [Serializable] public class StartSessionRequest  { public int memoreto_id; }

    [Serializable] public class StartSessionResponse
    {
        public string      session_id;
        public string      timestamp_start;
        public UserData    user;
        public MemoRetoData memoreto;
    }

    [Serializable] public class EndSessionRequest
    {
        public string session_id;
        public bool   completed;
    }

    [Serializable] public class AnswerItem
    {
        public string intersection_id;
        public int    value;
    }

    [Serializable] public class SubmitAnswerRequest
    {
        public int              id_memoreto;
        public int              attempt_number;
        public string           start_time;
        public string           end_time;
        public List<AnswerItem> answers;
    }

    [Serializable] public class ScoreBreakdown
    {
        public int attempt_max_score;
        public int time_penalty;
        public int final_score;
    }

    [Serializable] public class AnswerResultData
    {
        public int            score;
        public int            attempt_number;
        public bool           is_correct;
        public string         id_memoreto;
        public UserData       user;
        public int            ranking_position;
        public int            next_attempt_max_score;
        public ScoreBreakdown score_breakdown;
    }

    [Serializable] public class SubmitAnswerResponse
    {
        public string          status;
        public string          message;
        public AnswerResultData data_memoreto;
    }

    [Serializable] public class RankingEntry
    {
        public int    position;
        public string username;
        public int    score;
    }

    [Serializable] public class RankingResponse
    {
        public string             memoreto_id;
        public string             memoreto_name;
        public List<RankingEntry> ranking;
        public int                total_players;
    }
}
