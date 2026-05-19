import { useState } from 'react';
import { useAuthStore } from '@/store/authStore';
import { Modal } from '@/pages/components/Modal';
import { formStyle } from '@/styles/formStyle';

type Props = {
  visible: boolean;
  onClose: () => void;
};

export function RegisterModal({ visible, onClose }: Props) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [localError, setLocalError] = useState<string | undefined>(undefined);
  const { register, loading, error } = useAuthStore();

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLocalError(undefined);
    if (password !== confirm) {
      setLocalError('Slaptažodžiai nesutampa.');
      return;
    }
    const ok = await register(email, password);
    if (ok) onClose();
  };

  return (
    <Modal visibleModal={visible} setVisibleModal={() => onClose()} title="Registruotis">
      <form onSubmit={onSubmit} className="flex flex-col gap-3">
        {(localError || error) && <div className="text-red-600 text-sm">{localError ?? error}</div>}
        <div>
          <label className={formStyle.label}>El. paštas</label>
          <input className={formStyle.input} type="email" value={email}
                 onChange={(e) => setEmail(e.target.value)} required />
        </div>
        <div>
          <label className={formStyle.label}>Slaptažodis</label>
          <input className={formStyle.input} type="password" value={password}
                 onChange={(e) => setPassword(e.target.value)} required />
        </div>
        <div>
          <label className={formStyle.label}>Patvirtinti slaptažodį</label>
          <input className={formStyle.input} type="password" value={confirm}
                 onChange={(e) => setConfirm(e.target.value)} required />
        </div>
        <div className="flex gap-2">
          <button className={formStyle.button} type="submit" disabled={loading}>Registruotis</button>
          <button className={formStyle.button} type="button" onClick={onClose}>Uždaryti</button>
        </div>
      </form>
    </Modal>
  );
}