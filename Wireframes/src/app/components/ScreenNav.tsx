import { useNavigate, useLocation } from "react-router";

const SCREENS = [
  { label: "MENU PRINCIPAL", path: "/" },
  { label: "HUD DE TREINO", path: "/training" },
  { label: "TUTORIAL", path: "/tutorial" },
  { label: "MODO TESTE", path: "/test" },
];

export function ScreenNav() {
  const navigate = useNavigate();
  const location = useLocation();

  return (
    <div className="absolute bottom-6 left-1/2 -translate-x-1/2 z-50 flex items-center gap-1">
      <span className="text-white/25 text-[10px] mr-2 tracking-widest">TELAS</span>
      {SCREENS.map((s) => {
        const active = location.pathname === s.path;
        return (
          <button
            key={s.path}
            onClick={() => navigate(s.path)}
            className="px-3 py-1 text-[10px] tracking-widest transition-all border"
            style={{
              fontFamily: "'Space Mono', monospace",
              background: active ? "rgba(255,255,255,0.1)" : "transparent",
              borderColor: active ? "rgba(255,255,255,0.4)" : "rgba(255,255,255,0.12)",
              color: active ? "rgba(255,255,255,0.85)" : "rgba(255,255,255,0.3)",
              cursor: "pointer",
            }}
          >
            {s.label}
          </button>
        );
      })}
    </div>
  );
}
