import { useNavigate } from "react-router";
import { motion } from "motion/react";
import { WFBackground } from "./WFBackground";
import { ScreenNav } from "./ScreenNav";

function WFButton({
  label,
  sublabel,
  onClick,
  accent = false,
  danger = false,
}: {
  label: string;
  sublabel?: string;
  onClick: () => void;
  accent?: boolean;
  danger?: boolean;
}) {
  const borderColor = danger
    ? "rgba(255,80,80,0.45)"
    : accent
    ? "rgba(255,255,255,0.55)"
    : "rgba(255,255,255,0.2)";

  const hoverBg = danger
    ? "rgba(255,80,80,0.08)"
    : accent
    ? "rgba(255,255,255,0.08)"
    : "rgba(255,255,255,0.04)";

  return (
    <motion.button
      onClick={onClick}
      whileHover={{ scale: 1.02 }}
      whileTap={{ scale: 0.98 }}
      className="relative w-full group"
      style={{
        fontFamily: "'Space Mono', monospace",
        background: "transparent",
        cursor: "pointer",
      }}
    >
      <div
        className="relative flex items-center px-8 py-5 transition-all duration-200"
        style={{ border: `1px dashed ${borderColor}` }}
      >
        {/* Hover fill */}
        <motion.div
          className="absolute inset-0"
          initial={{ opacity: 0 }}
          whileHover={{ opacity: 1 }}
          style={{ background: hoverBg }}
        />

        {/* Corner accents */}
        <div className="absolute top-0 left-0 w-2 h-2 border-t-2 border-l-2" style={{ borderColor }} />
        <div className="absolute top-0 right-0 w-2 h-2 border-t-2 border-r-2" style={{ borderColor }} />
        <div className="absolute bottom-0 left-0 w-2 h-2 border-b-2 border-l-2" style={{ borderColor }} />
        <div className="absolute bottom-0 right-0 w-2 h-2 border-b-2 border-r-2" style={{ borderColor }} />

        <div className="relative z-10 flex-1 text-left">
          <div
            className="tracking-[0.25em] text-sm"
            style={{
              color: danger
                ? "rgba(255,100,100,0.8)"
                : accent
                ? "rgba(255,255,255,0.9)"
                : "rgba(255,255,255,0.65)",
            }}
          >
            {label}
          </div>
          {sublabel && (
            <div
              className="text-[10px] tracking-widest mt-0.5"
              style={{ color: "rgba(255,255,255,0.28)" }}
            >
              {sublabel}
            </div>
          )}
        </div>

        <div
          className="relative z-10 text-xs tracking-widest"
          style={{ color: danger ? "rgba(255,100,100,0.35)" : "rgba(255,255,255,0.2)" }}
        >
          [SELECIONAR]
        </div>
      </div>
    </motion.button>
  );
}

export function MainMenu() {
  const navigate = useNavigate();

  return (
    <WFBackground>
      {/* Version label */}
      <div
        className="absolute top-6 right-12 text-[10px] tracking-widest"
        style={{ color: "rgba(255,255,255,0.18)", fontFamily: "'Space Mono', monospace" }}
      >
        VR-RCP // WIREFRAME v0.1
      </div>

      {/* Left annotation */}
      <div
        className="absolute left-6 top-1/2 -translate-y-1/2 -rotate-90 text-[10px] tracking-[0.35em] origin-center"
        style={{ color: "rgba(255,255,255,0.12)", fontFamily: "'Space Mono', monospace" }}
      >
        PAINEL DE NAVEGAÇÃO PRINCIPAL
      </div>

      {/* Center panel */}
      <div className="absolute inset-0 flex flex-col items-center justify-center px-4">
        <motion.div
          initial={{ opacity: 0, y: 16 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          className="w-full max-w-md"
        >
          {/* Title block */}
          <div className="text-center mb-10">
            {/* Medical cross icon — wireframe style */}
            <div className="flex items-center justify-center mb-6">
              <div className="relative w-12 h-12">
                <div
                  className="absolute top-1/2 left-0 right-0 h-px"
                  style={{ background: "rgba(255,255,255,0.5)", transform: "translateY(-50%)" }}
                />
                <div
                  className="absolute left-1/2 top-0 bottom-0 w-px"
                  style={{ background: "rgba(255,255,255,0.5)", transform: "translateX(-50%)" }}
                />
                <div
                  className="absolute inset-2 border"
                  style={{ borderColor: "rgba(255,255,255,0.2)" }}
                />
              </div>
            </div>

            <div
              className="text-[11px] tracking-[0.5em] mb-2"
              style={{ color: "rgba(255,255,255,0.3)", fontFamily: "'Space Mono', monospace" }}
            >
              // SISTEMA DE TREINO VR //
            </div>
            <h1
              className="text-3xl tracking-[0.18em] mb-1"
              style={{ color: "rgba(255,255,255,0.92)", fontFamily: "'Space Mono', monospace" }}
            >
              TREINO DE RCP
            </h1>
            <div
              className="text-[10px] tracking-[0.4em]"
              style={{ color: "rgba(255,255,255,0.25)", fontFamily: "'Space Mono', monospace" }}
            >
              RESSUSCITAÇÃO CARDIOPULMONAR
            </div>

            {/* Decorative divider */}
            <div className="flex items-center gap-3 mt-5 justify-center">
              <div className="h-px flex-1 max-w-16" style={{ background: "rgba(255,255,255,0.12)" }} />
              <div className="w-1.5 h-1.5 rotate-45" style={{ background: "rgba(255,255,255,0.2)" }} />
              <div className="h-px flex-1 max-w-16" style={{ background: "rgba(255,255,255,0.12)" }} />
            </div>
          </div>

          {/* Buttons */}
          <div className="flex flex-col gap-3">
            <WFButton
              label="INICIAR TREINO"
              sublabel="simulação guiada · modo iniciante"
              onClick={() => navigate("/training")}
              accent
            />
            <WFButton
              label="MODO TESTE"
              sublabel="avaliação de desempenho · cronometrado"
              onClick={() => navigate("/test")}
            />
            <WFButton
              label="TUTORIAL"
              sublabel="instrução passo a passo · aprender"
              onClick={() => navigate("/tutorial")}
            />
            <WFButton
              label="SAIR"
              sublabel="fechar aplicação"
              onClick={() => {}}
              danger
            />
          </div>

          {/* Annotation below */}
          <div
            className="mt-6 text-center text-[10px] tracking-widest"
            style={{ color: "rgba(255,255,255,0.15)", fontFamily: "'Space Mono', monospace" }}
          >
            USE O GATILHO DO CONTROLE PARA SELECIONAR
          </div>
        </motion.div>
      </div>

      {/* Floating annotation */}
      <div
        className="absolute right-10 top-1/3 flex flex-col gap-1 text-[9px] tracking-widest"
        style={{ color: "rgba(255,255,255,0.14)", fontFamily: "'Space Mono', monospace" }}
      >
        <div>┌── LARGURA DO PAINEL: 440px</div>
        <div>├── BOTÕES: 4</div>
        <div>├── FONTE: SPACE MONO</div>
        <div>└── PROFUNDIDADE: Z+0,5m</div>
      </div>

      <ScreenNav />
    </WFBackground>
  );
}
