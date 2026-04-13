import { motion, AnimatePresence } from "motion/react";
import { useNavigate } from "react-router";

interface PauseMenuProps {
  isOpen: boolean;
  onResume: () => void;
  onRestart: () => void;
}

function PauseButton({
  label,
  desc,
  onClick,
  variant = "default",
}: {
  label: string;
  desc?: string;
  onClick: () => void;
  variant?: "default" | "accent" | "danger";
}) {
  const colors = {
    default: {
      border: "rgba(255,255,255,0.2)",
      text: "rgba(255,255,255,0.6)",
      hover: "rgba(255,255,255,0.05)",
    },
    accent: {
      border: "rgba(255,255,255,0.5)",
      text: "rgba(255,255,255,0.9)",
      hover: "rgba(255,255,255,0.08)",
    },
    danger: {
      border: "rgba(255,90,90,0.4)",
      text: "rgba(255,100,100,0.75)",
      hover: "rgba(255,60,60,0.07)",
    },
  }[variant];

  return (
    <motion.button
      onClick={onClick}
      whileHover={{ scale: 1.02 }}
      whileTap={{ scale: 0.97 }}
      className="w-full relative group text-left"
      style={{ fontFamily: "'Space Mono', monospace", cursor: "pointer", background: "transparent" }}
    >
      <div
        className="px-6 py-4 transition-all duration-150 relative"
        style={{ border: `1px dashed ${colors.border}` }}
      >
        <motion.div
          className="absolute inset-0"
          initial={{ opacity: 0 }}
          whileHover={{ opacity: 1 }}
          style={{ background: colors.hover }}
        />
        <div className="relative z-10">
          <div className="text-sm tracking-[0.22em]" style={{ color: colors.text }}>
            {label}
          </div>
          {desc && (
            <div className="text-[10px] tracking-widest mt-0.5" style={{ color: "rgba(255,255,255,0.22)" }}>
              {desc}
            </div>
          )}
        </div>
      </div>
    </motion.button>
  );
}

export function PauseMenu({ isOpen, onResume, onRestart }: PauseMenuProps) {
  const navigate = useNavigate();

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          key="pause-overlay"
          className="absolute inset-0 z-50 flex items-center justify-center"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          style={{ background: "rgba(5,8,15,0.82)", backdropFilter: "blur(4px)" }}
        >
          {/* Scanline overlay */}
          <div
            className="absolute inset-0 pointer-events-none"
            style={{
              backgroundImage:
                "repeating-linear-gradient(0deg, transparent, transparent 3px, rgba(255,255,255,0.012) 4px)",
            }}
          />

          <motion.div
            key="pause-panel"
            className="relative w-80"
            initial={{ scale: 0.94, opacity: 0, y: 12 }}
            animate={{ scale: 1, opacity: 1, y: 0 }}
            exit={{ scale: 0.96, opacity: 0, y: 8 }}
            transition={{ duration: 0.25 }}
          >
            {/* Panel outer border */}
            <div className="p-0.5" style={{ border: "1px solid rgba(255,255,255,0.1)" }}>
              <div
                className="p-6"
                style={{
                  border: "1px dashed rgba(255,255,255,0.08)",
                  background: "rgba(5,8,15,0.6)",
                }}
              >
                {/* Corner brackets */}
                <div className="absolute top-2 left-2 w-3 h-3 border-t border-l border-white/30" />
                <div className="absolute top-2 right-2 w-3 h-3 border-t border-r border-white/30" />
                <div className="absolute bottom-2 left-2 w-3 h-3 border-b border-l border-white/30" />
                <div className="absolute bottom-2 right-2 w-3 h-3 border-b border-r border-white/30" />

                {/* Header */}
                <div className="text-center mb-6">
                  <div
                    className="text-[10px] tracking-[0.5em] mb-1"
                    style={{ color: "rgba(255,255,255,0.25)", fontFamily: "'Space Mono', monospace" }}
                  >
                    // SISTEMA //
                  </div>
                  <div
                    className="text-2xl tracking-[0.3em]"
                    style={{ color: "rgba(255,255,255,0.88)", fontFamily: "'Space Mono', monospace" }}
                  >
                    PAUSADO
                  </div>

                  {/* Animated pause bars */}
                  <div className="flex items-center justify-center gap-2 mt-3">
                    <motion.div
                      className="w-1.5 h-5"
                      style={{ background: "rgba(255,255,255,0.3)" }}
                      animate={{ opacity: [0.3, 0.8, 0.3] }}
                      transition={{ repeat: Infinity, duration: 1.2 }}
                    />
                    <motion.div
                      className="w-1.5 h-5"
                      style={{ background: "rgba(255,255,255,0.3)" }}
                      animate={{ opacity: [0.3, 0.8, 0.3] }}
                      transition={{ repeat: Infinity, duration: 1.2, delay: 0.15 }}
                    />
                  </div>
                </div>

                {/* Stats snapshot */}
                <div
                  className="p-3 mb-5"
                  style={{
                    border: "1px dashed rgba(255,255,255,0.1)",
                    background: "rgba(255,255,255,0.025)",
                  }}
                >
                  <div
                    className="text-[9px] tracking-[0.4em] mb-2"
                    style={{ color: "rgba(255,255,255,0.25)", fontFamily: "'Space Mono', monospace" }}
                  >
                    RESUMO DA SESSÃO
                  </div>
                  <div className="grid grid-cols-3 gap-2">
                    {[
                      { label: "COMPRESSÕES", val: "24" },
                      { label: "PROF. MÉDIA", val: "4,8cm" },
                      { label: "PRECISÃO", val: "76%" },
                    ].map((s) => (
                      <div key={s.label} className="text-center">
                        <div
                          className="text-sm"
                          style={{ color: "rgba(255,255,255,0.7)", fontFamily: "'Space Mono', monospace" }}
                        >
                          {s.val}
                        </div>
                        <div
                          className="text-[8px] tracking-wider mt-0.5"
                          style={{ color: "rgba(255,255,255,0.2)", fontFamily: "'Space Mono', monospace" }}
                        >
                          {s.label}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Buttons */}
                <div className="flex flex-col gap-2">
                  <PauseButton
                    label="RETOMAR"
                    desc="continuar sessão atual"
                    onClick={onResume}
                    variant="accent"
                  />
                  <PauseButton
                    label="REINICIAR"
                    desc="reiniciar sessão · manter configurações"
                    onClick={onRestart}
                  />
                  <PauseButton
                    label="CONFIGURAÇÕES"
                    desc="ajustar dificuldade · áudio"
                    onClick={() => {}}
                  />
                  <PauseButton
                    label="SAIR PARA O MENU"
                    desc="voltar ao menu principal"
                    onClick={() => navigate("/")}
                    variant="danger"
                  />
                </div>
              </div>
            </div>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}
