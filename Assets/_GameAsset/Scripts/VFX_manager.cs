using System;
using System.Collections.Generic;
using UnityEngine;

public class VFX_manager : Singleton<VFX_manager>
{
    [Serializable]
    public struct VFXInfo
    {
        public GameObject VFXPrefab;
        public VFXKEY Vfxkey;
    }

    [SerializeField]
    private VFXInfo[] _vfxInfos;

    private List<VFXObject> m_vfxObjects = new List<VFXObject>();

    private class VFXObject
    {
        private ParticleSystem _particle;
        private GameObject _obj;
        private bool _haveParticle;
        private VFXKEY Vfxkey;

        [Obsolete("Obsolete")]
        public VFXObject(GameObject obj, VFXKEY key)
        {
            _obj = obj;
            Vfxkey = key;
            _haveParticle = _obj.TryGetComponent(out ParticleSystem particleSystem);
            if (_haveParticle)
            {
                _particle = particleSystem;
            }
        }

        public bool IsReady(VFXKEY key)
        {
            return _obj.activeSelf && Vfxkey == key && _haveParticle && !_particle.isPlaying;
        }

        public void SetPos(Vector3 pos)
        {
            _obj.transform.position = pos;
        }

        public void SetPosRot(Vector3 pos, Vector3 dir)
        {
            SetPos(pos);
            _obj.transform.forward = dir;
        }

        public void SetMaterial(Material material)
        {
            if(!_haveParticle || material == null) return;
            Renderer renderer = _particle.GetComponent<Renderer>();
            renderer.material = material;
        }

        public void Emit(int count)
        {
            if (!_haveParticle)
            {
                return;
            }

            _obj.SetActive(true);
            _particle.Emit(count);
        }

        public void Reset()
        {
            if(!_haveParticle) return;
            _obj.SetActive(true);
            _particle.Stop();
            _particle.Play();
        }

        public void SetLifeTimeByDistance(float distance)
        {
            if (_haveParticle)
            {
                ParticleSystem.MainModule main = _particle.main;
                main.startLifetimeMultiplier = distance / main.startSpeedMultiplier;
            }
        }
    }
    
    
    
    [Obsolete("Obsolete")]
    public void PlayEffect(Vector3 position, Vector3 direction,Material material, VFXKEY key)
    {
        VFXObject vfxObj = GetVFXObj(key);
        if (vfxObj == null)
        {
            return;
        }

        vfxObj.SetPosRot(position, direction);
        vfxObj.SetMaterial(material);
        vfxObj.Reset();
    }
    
    [Obsolete("Obsolete")]
    public void PlayEffect(Vector3 position,Material material, VFXKEY key)
    {
        VFXObject vfxObj = GetVFXObj(key);
        if (vfxObj == null)
        {
            return;
        }

        vfxObj.SetPos(position);
        vfxObj.SetMaterial(material);
        vfxObj.Reset();
    }

    [Obsolete("Obsolete")]
    private VFXObject GetVFXObj(VFXKEY key)
    {
        VFXObject vfxObj = m_vfxObjects.Find(x => x.IsReady(key));
        if (vfxObj == null)
        {
            int index = Array.FindIndex(_vfxInfos, x => x.Vfxkey == key);
            if (index < 0)
            {
                return null;
            }

            vfxObj = new VFXObject(Instantiate(_vfxInfos[index].VFXPrefab, transform), _vfxInfos[index].Vfxkey);
            m_vfxObjects.Add(vfxObj);
        }

        return vfxObj;
    }
}

[Serializable]
public enum VFXKEY
{
    HitEffect,
    EnemyDead
}