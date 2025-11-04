import { useState } from 'react';
import { useAuthStore } from '@/store/authStore';
import { Modal } from '@/pages/components/Modal';
import { formStyle } from '@/styles/formStyle';

type Props = {
  visible: boolean;
  onClose: () => void;
};

export function LoginModal({ visible, onClose }: Props) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const { login, loading, error } = useAuthStore();

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const ok = await login(email, password, rememberMe);
    if (ok) onClose();
  };

  return (
    <Modal visibleModal={visible} setVisibleModal={() => onClose()} title="Prisijungti">
      <form onSubmit={onSubmit} className="flex flex-col gap-3">
        {error && <div className="text-red-600 text-sm">{error}</div>}
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
        <label className="inline-flex items-center gap-2">
          <input type="checkbox" checked={rememberMe} onChange={(e) => setRememberMe(e.target.checked)} />
          <span>Prisiminti</span>
        </label>
        <div className="flex gap-2">
          <button className={formStyle.button} type="submit" disabled={loading}>Prisijungti</button>
          <button className={formStyle.button} type="button" onClick={onClose}>Uždaryti</button>
        </div>
      </form>
    </Modal>
  );
}