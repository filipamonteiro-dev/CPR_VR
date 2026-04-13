import { useState } from "react";
import { useNavigate } from "react-router";
import { motion, AnimatePresence } from "motion/react";
import { WFBackground } from "./WFBackground";
import { ScreenNav } from "./ScreenNav";
import { PatientSilhouette } from "./PatientSilhouette";

interface Step {
  id: number;
  label: string;
  title: string;
  instruction: string;
  highlight: "full" | "chest" | "hands" | "head" | "none";
  showArrow: boolean;
  showHandPlacement: boolean;
  annotations: { x: string; y: string; text: string; dir: "left" | "right" | "up" | "down" }[];
}

const STEPS: Step[] = [
  {
    id: 1,
    label: "VERIFICAÇÃO DO LOCAL",
    title: "VERIFIQUE O LOCAL",
    instruction:
      "Certifique-se de que a área é segura antes de se aproximar. Procure possíveis perigos. Confirme que o paciente está inconsciente e não responde.",
    highlight: "full",
    showArrow: false,
    showHandPlacement: false,
    annotations: [
      { x: "15%", y: "30%", text: "VERIFICAR AMBIENTE", dir: "right" },
      { x: "75%", y: "55%", text: "CHECAR RESPOSTA", dir: "left" },
    ],
  },
  {
    id: 2,
    label: "PEDIR AJUDA",
    title: "LIGAR PARA EMERGÊNCIA",
    instruction:
      "Ligue para o SAMU (192) ou instrua alguém a ligar. Solicite um DEA se disponível. Posicione o paciente em decúbito dorsal sobre uma superfície firme.",
    highlight: "none",
    showArrow: false,
    showHandPlacement: false,
    annotations: [
      { x: "18%", y: "45%", text: "SUPERFÍCIE FIRME", dir: "right" },
      { x: "72%", y: "30%", text: "LIGUE 192", dir: "left" },
    ],
  },
  {
    id: 3,
    label: "LOCAL DO TÓRAX",
    title: "LOCALIZAR ZONA DE COMPRESSÃO",
    instruction:
      "Encontre a metade inferior do esterno. Coloque o calcanhar de uma mão no centro do peito do paciente, entre os mamilos.",
    highlight: "chest",
    showArrow: false,
    showHandPlacement: false,
    annotations: [
      { x: "16%", y: "42%", text: "PARTE INF. DO ESTERNO", dir: "right" },
      { x: "70%", y: "42%", text: "CENTRO DO PEITO", dir: "left" },
    ],
  },
  {
    id: 4,
    label: "POSIÇÃO DAS MÃOS",
    title: "POSICIONE SUAS MÃOS",
    instruction:
      "Coloque o calcanhar de uma mão na zona de compressão. Sobreponha a outra mão por cima. Entrelace os dedos e mantenha-os levantados sem tocar o tórax.",
    highlight: "chest",
    showArrow: false,
    showHandPlacement: true,
    annotations: [
      { x: "14%", y: "38%", text: "CALCANHAR DA MÃO", dir: "right" },
      { x: "68%", y: "38%", text: "DEDOS LEVANTADOS", dir: "left" },
    ],
  },
  {
    id: 5,
    label: "COMPRESSÕES",
    title: "INICIAR COMPRESSÕES",
    instruction:
      "Pressione forte e rápido — pelo menos 5cm de profundidade a 100–110 BPM. Permita o retorno completo do tórax entre cada compressão.",
    highlight: "chest",
    showArrow: true,
    showHandPlacement: true,
    annotations: [
      { x: "14%", y: "30%", text: "PRESSIONAR 5–6cm", dir: "right" },
      { x: "68%", y: "30%", text: "100–110 BPM", dir: "left" },
    ],
  },
  {
    id: 6,
    label: "CICLO CONTÍNUO",
    title: "MANTER O CICLO",
    instruction:
      "Realize 30 compressões seguidas de 2 ventilações de resgate. Continue o ciclo até a chegada do DEA ou de socorro especializado.",
    highlight: "chest",
    showArrow: true,
    showHandPlacement: true,
    annotations: [
      { x: "14%", y: "35%", text: "30 COMPRESSÕES", dir: "right" },
      { x: "68%", y: "35%", text: "2 VENTILAÇÕES", dir: "left" },
    ],
  },
];

function StepDot({ active, complete, index }: { active: boolean; complete: boolean; index: number }) {
  return (
    <div className="flex flex-col items-center gap-1">
      <div
        className="relative flex items-center justify-center transition-all duration-300"
        style={{
          width: active ? "28px" : "20px",
          height: active ? "28px" : "20px",
          border: `1px ${active ? "solid" : "dashed"} ${
            complete
              ? "rgba(255,255,255,0.6)"
              : active
              ? "rgba(255,255,255,0.8)"
              : "rgba(255,255,255,0.2)"
          }`,
          background: complete
            ? "rgba(255,255,255,0.15)"
            : active
            ? "rgba(255,255,255,0.08)"
            : "transparent",
        }}
      >
        {complete ? (
          <span style={{ color: "rgba(255,255,255,0.7)", fontSize: "10px" }}>✓</span>
        ) : (
          <span
            style={{
              color: active ? "rgba(255,255,255,0.9)" : "rgba(255,255,255,0.25)",
              fontSize: "10px",
              fontFamily: "'Space Mono', monospace",
            }}
          >
            {index + 1}
          </span>
        )}
      </div>
    </div>
  );
}

function ArrowAnnotation({
  x,
  y,
  text,
  dir,
}: {
  x: string;
  y: string;
  text: string;
  dir: "left" | "right" | "up" | "down";
}) {
  const lineLength = 48;
  const arrowOffset = {
    left:  { dx: -lineLength, dy: 0, textAnchor: "end",    textX: -lineLength - 6, textY: 4 },
    right: { dx: lineLength,  dy: 0, textAnchor: "start",  textX: lineLength + 6,  textY: 4 },
    up:    { dx: 0, dy: -lineLength, textAnchor: "middle", textX: 0, textY: -lineLength - 8 },
    down:  { dx: 0, dy: lineLength,  textAnchor: "middle", textX: 0, textY: lineLength + 14 },
  }[dir];

  return (
    <motion.div
      className="absolute"
      style={{ left: x, top: y, transform: "translate(-50%, -50%)" }}
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      transition={{ duration: 0.4 }}
    >
      <svg
        width="160"
        height="80"
        style={{ overflow: "visible", position: "absolute", left: "-80px", top: "-40px" }}
      >
        <motion.line
          x1="80" y1="40"
          x2={80 + arrowOffset.dx}
          y2={40 + arrowOffset.dy}
          stroke="rgba(255,255,255,0.3)"
          strokeWidth="1"
          strokeDasharray="4,3"
          initial={{ pathLength: 0 }}
          animate={{ pathLength: 1 }}
          transition={{ duration: 0.5 }}
        />
        <motion.circle
          cx="80" cy="40" r="3"
          fill="rgba(255,255,255,0.4)"
          animate={{ scale: [1, 1.3, 1] }}
          transition={{ repeat: Infinity, duration: 1.8 }}
        />
        <text
          x={80 + arrowOffset.textX}
          y={40 + arrowOffset.textY}
          textAnchor={arrowOffset.textAnchor}
          fill="rgba(255,255,255,0.35)"
          fontSize="7.5"
          fontFamily="'Space Mono', monospace"
          letterSpacing="1.5"
        >
          {text}
        </text>
      </svg>
    </motion.div>
  );
}

export function Tutorial() {
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState(0);
  const step = STEPS[currentStep];

  const goNext = () => {
    if (currentStep < STEPS.length - 1) setCurrentStep((s) => s + 1);
    else navigate("/training");
  };

  const goPrev = () => {
    if (currentStep > 0) setCurrentStep((s) => s - 1);
  };

  return (
    <WFBackground>
      <div className="relative w-full h-full flex flex-col">
        {/* Header bar */}
        <div
          className="relative z-10 flex items-center justify-between px-8 py-4"
          style={{ borderBottom: "1px solid rgba(255,255,255,0.06)" }}
        >
          <div style={{ fontFamily: "'Space Mono', monospace" }}>
            <div className="text-[9px] tracking-[0.5em]" style={{ color: "rgba(255,255,255,0.22)" }}>
              // MÓDULO TUTORIAL //
            </div>
            <div className="text-sm tracking-widest" style={{ color: "rgba(255,255,255,0.75)" }}>
              PROCEDIMENTO DE RCP
            </div>
          </div>

          {/* Step progress dots */}
          <div className="flex items-center gap-2">
            {STEPS.map((s, i) => (
              <StepDot
                key={s.id}
                active={i === currentStep}
                complete={i < currentStep}
                index={i}
              />
            ))}
          </div>

          <div style={{ fontFamily: "'Space Mono', monospace" }}>
            <div className="text-[9px] tracking-[0.4em] text-right" style={{ color: "rgba(255,255,255,0.22)" }}>
              ETAPA
            </div>
            <div className="text-sm tracking-widest text-right" style={{ color: "rgba(255,255,255,0.75)" }}>
              {currentStep + 1} / {STEPS.length}
            </div>
          </div>
        </div>

        {/* Main content */}
        <div className="relative flex-1 flex items-center justify-center overflow-hidden">

          {/* Patient silhouette — horizontal, wide */}
          <div
            className="relative flex items-center justify-center"
            style={{ width: "min(600px, 65vw)", maxWidth: "600px" }}
          >
            <PatientSilhouette
              highlightChest={step.highlight === "chest"}
              showHandPlacement={step.showHandPlacement}
              compressDepth={step.showArrow ? 0.5 : 0}
              showArrow={step.showArrow}
            />

            {/* Full body highlight glow */}
            {step.highlight === "full" && (
              <motion.div
                className="absolute inset-0 pointer-events-none"
                initial={{ opacity: 0 }}
                animate={{ opacity: [0, 0.08, 0] }}
                transition={{ repeat: Infinity, duration: 2.5 }}
                style={{ border: "1px dashed rgba(255,255,255,0.4)", borderRadius: "4px" }}
              />
            )}
          </div>

          {/* Arrow annotations */}
          <AnimatePresence mode="wait">
            {step.annotations.map((ann, i) => (
              <ArrowAnnotation key={`${step.id}-${i}`} {...ann} />
            ))}
          </AnimatePresence>

          {/* Left panel: step number & label */}
          <div
            className="absolute left-6 top-1/2 -translate-y-1/2"
            style={{ fontFamily: "'Space Mono', monospace" }}
          >
            <div
              className="text-[9px] tracking-[0.4em] mb-1"
              style={{ color: "rgba(255,255,255,0.2)" }}
            >
              ETAPA DO PROCEDIMENTO
            </div>
            <motion.div
              key={currentStep}
              initial={{ opacity: 0, x: -8 }}
              animate={{ opacity: 1, x: 0 }}
              className="text-5xl"
              style={{ color: "rgba(255,255,255,0.08)", lineHeight: 1 }}
            >
              0{step.id}
            </motion.div>
            <div
              className="text-[10px] tracking-[0.35em] mt-2"
              style={{ color: "rgba(255,255,255,0.35)" }}
            >
              {step.label}
            </div>

            {/* Legend */}
            <div className="mt-8 flex flex-col gap-2">
              {[
                { color: "rgba(255,255,255,0.5)", dash: true, label: "ZONA DE INTERAÇÃO" },
                { color: "rgba(255,255,255,0.25)", dash: false, label: "CONTORNO DO CORPO" },
                { color: "rgba(255,255,255,0.3)", dash: true, label: "GUIA DE SETA" },
              ].map((l) => (
                <div key={l.label} className="flex items-center gap-2">
                  <div
                    className="w-5 h-px"
                    style={{
                      background: l.color,
                      borderTop: l.dash ? `1px dashed ${l.color}` : "none",
                    }}
                  />
                  <span
                    className="text-[8px] tracking-widest"
                    style={{ color: "rgba(255,255,255,0.2)" }}
                  >
                    {l.label}
                  </span>
                </div>
              ))}
            </div>
          </div>

          {/* Right panel: interaction cues */}
          <div
            className="absolute right-6 top-1/2 -translate-y-1/2 flex flex-col gap-3"
            style={{ width: "160px", fontFamily: "'Space Mono', monospace" }}
          >
            {/* Compression spec — only when arrow shown */}
            {step.showArrow && (
              <div
                className="p-3"
                style={{
                  border: "1px dashed rgba(255,255,255,0.15)",
                  background: "rgba(5,8,15,0.5)",
                }}
              >
                <div
                  className="text-[8px] tracking-widest mb-2"
                  style={{ color: "rgba(255,255,255,0.22)" }}
                >
                  SPEC. DE COMPRESSÃO
                </div>
                {[
                  { label: "PROFUNDIDADE", value: "5–6 cm" },
                  { label: "TAXA", value: "100–110/min" },
                  { label: "RETORNO", value: "COMPLETO" },
                  { label: "CICLO", value: "30 : 2" },
                ].map((m) => (
                  <div key={m.label} className="flex justify-between items-center py-0.5">
                    <span className="text-[8px] tracking-wider" style={{ color: "rgba(255,255,255,0.25)" }}>
                      {m.label}
                    </span>
                    <span className="text-[9px] tracking-wider" style={{ color: "rgba(255,255,255,0.6)" }}>
                      {m.value}
                    </span>
                  </div>
                ))}
              </div>
            )}

            {/* VR interaction hint */}
            <div
              className="p-3"
              style={{
                border: "1px dashed rgba(255,255,255,0.1)",
                background: "rgba(5,8,15,0.5)",
              }}
            >
              <div
                className="text-[8px] tracking-widest mb-1"
                style={{ color: "rgba(255,255,255,0.22)" }}
              >
                INTERAÇÃO VR
              </div>
              <div className="text-[9px] tracking-wide" style={{ color: "rgba(255,255,255,0.4)" }}>
                OLHE PARA A ZONA DESTACADA PARA CONFIRMAR
              </div>
            </div>

            {/* Progress bar */}
            <div>
              <div
                className="text-[8px] tracking-widest mb-1"
                style={{ color: "rgba(255,255,255,0.2)" }}
              >
                PROGRESSO
              </div>
              <div
                className="h-1.5 w-full relative"
                style={{
                  background: "rgba(255,255,255,0.06)",
                  border: "1px solid rgba(255,255,255,0.1)",
                }}
              >
                <motion.div
                  className="h-full"
                  animate={{ width: `${((currentStep + 1) / STEPS.length) * 100}%` }}
                  transition={{ duration: 0.4 }}
                  style={{ background: "rgba(255,255,255,0.4)" }}
                />
              </div>
            </div>
          </div>
        </div>

        {/* Bottom instruction panel */}
        <div
          className="relative z-10"
          style={{ borderTop: "1px solid rgba(255,255,255,0.06)" }}
        >
          <div className="px-8 py-5 flex items-center gap-6">
            {/* Instruction text */}
            <div className="flex-1">
              <AnimatePresence mode="wait">
                <motion.div
                  key={currentStep}
                  initial={{ opacity: 0, y: 6 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -6 }}
                  transition={{ duration: 0.3 }}
                >
                  <div
                    className="text-sm tracking-[0.15em] mb-1"
                    style={{ color: "rgba(255,255,255,0.82)", fontFamily: "'Space Mono', monospace" }}
                  >
                    {step.title}
                  </div>
                  <div
                    className="text-xs leading-relaxed"
                    style={{
                      color: "rgba(255,255,255,0.4)",
                      fontFamily: "'Space Mono', monospace",
                      maxWidth: "520px",
                    }}
                  >
                    {step.instruction}
                  </div>
                </motion.div>
              </AnimatePresence>
            </div>

            {/* Navigation buttons */}
            <div className="flex items-center gap-3 shrink-0">
              <motion.button
                onClick={goPrev}
                disabled={currentStep === 0}
                whileHover={currentStep > 0 ? { scale: 1.04 } : {}}
                whileTap={currentStep > 0 ? { scale: 0.96 } : {}}
                style={{
                  fontFamily: "'Space Mono', monospace",
                  background: "transparent",
                  border: "1px dashed rgba(255,255,255,0.18)",
                  color: currentStep === 0 ? "rgba(255,255,255,0.15)" : "rgba(255,255,255,0.55)",
                  cursor: currentStep === 0 ? "not-allowed" : "pointer",
                  padding: "10px 20px",
                  fontSize: "11px",
                  letterSpacing: "3px",
                }}
              >
                ← ANTERIOR
              </motion.button>

              <motion.button
                onClick={goNext}
                whileHover={{ scale: 1.04 }}
                whileTap={{ scale: 0.96 }}
                style={{
                  fontFamily: "'Space Mono', monospace",
                  background: "rgba(255,255,255,0.08)",
                  border: "1px solid rgba(255,255,255,0.35)",
                  color: "rgba(255,255,255,0.88)",
                  cursor: "pointer",
                  padding: "10px 24px",
                  fontSize: "11px",
                  letterSpacing: "3px",
                }}
              >
                {currentStep === STEPS.length - 1 ? "INICIAR TREINO →" : "PRÓXIMO →"}
              </motion.button>
            </div>
          </div>
        </div>

        <ScreenNav />
      </div>
    </WFBackground>
  );
}
