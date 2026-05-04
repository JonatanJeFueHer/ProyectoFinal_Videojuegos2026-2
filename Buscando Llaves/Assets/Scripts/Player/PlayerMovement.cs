using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Movimiento por casillas para juego por turnos.
/// Uso:
/// - Otro script debe llamar StartMove(steps, pathFromCurrent) donde
///   pathFromCurrent es la lista ordenada de casillas (Transforms) comenzando
///   por la siguiente casilla después de la posición actual.
/// - El jugador elige una casilla válida (dentro de 'steps') haciendo click.
/// - El script se encarga del desplazamiento suave y notifica cuando termina.
/// Requisitos en las casillas: Collider (para recibir clicks). Opcional: Renderer para resaltado.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento en unidades/segundo")]
    public float moveSpeed = 3f;

    [Tooltip("Tolerancia de llegada a la posición de la casilla (en metros)")]
    public float arriveThreshold = 0.05f;

    [Header("Click / Selección")]
    [Tooltip("Capa donde están las casillas (para raycast)")]
    public LayerMask tileLayerMask = ~0;

    // Estado interno
    enum State { Idle, SelectingTarget, Moving }
    State state = State.Idle;

    // Path y opciones para la selección
    List<Transform> currentPath = new List<Transform>();
    int allowedSteps = 0;

    // Movimiento en curso
    bool isMoving => state == State.Moving;

    // Eventos
    public Action OnMovementComplete;

    // Cache
    Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (state == State.SelectingTarget)
        {
            HandleSelectionInput();
        }
    }

    /// <summary>
    /// Comienza el proceso de movimiento después de un tiro de dado.
    /// 'steps' indica cuántas casillas puede avanzar el jugador.
    /// 'pathFromCurrent' es la lista ordenada de casillas desde la siguiente casilla
    /// hasta la meta posible (por ejemplo: el tablero deberá proporcionar estas casillas).
    /// </summary>
    public void StartMove(int steps, List<Transform> pathFromCurrent)
    {
        if (isMoving)
            return;

        if (pathFromCurrent == null)
            throw new ArgumentNullException(nameof(pathFromCurrent));

        allowedSteps = Mathf.Max(0, steps);
        currentPath = new List<Transform>(pathFromCurrent);
        EnterSelectionMode();
    }

    void EnterSelectionMode()
    {
        state = State.SelectingTarget;
        HighlightAllowedTiles(true);
    }

    void ExitSelectionMode()
    {
        HighlightAllowedTiles(false);
        state = State.Idle;
    }

    void HandleSelectionInput()
    {
        if (Input.GetMouseButtonDown(0) == false)
            return;

        if (mainCam == null)
            mainCam = Camera.main;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tileLayerMask))
        {
            Transform clicked = hit.collider.transform;
            int index = currentPath.IndexOf(clicked);
            if (index >= 0 && index < allowedSteps)
            {
                // Valid target seleccionado
                StartCoroutine(MoveAlongPathCoroutine(index));
            }
            else
            {
                // Click fuera del rango permitido -> puedes añadir feedback si quieres
            }
        }
    }

    IEnumerator MoveAlongPathCoroutine(int targetIndex)
    {
        state = State.Moving;
        HighlightAllowedTiles(false);

        // Mover secuencialmente desde la casilla más cercana hasta la seleccionada.
        // Se asume currentPath[0] = siguiente casilla desde la posición actual.
        for (int i = 0; i <= targetIndex; i++)
        {
            Vector3 targetPos = currentPath[i].position;
            // Mantener la altura del jugador si fuera necesario:
            targetPos.y = transform.position.y;

            while (Vector3.SqrMagnitude(transform.position - targetPos) > arriveThreshold * arriveThreshold)
            {
                Vector3 dir = (targetPos - transform.position).normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                yield return null;
            }

            // Asegurar posición exacta sobre la casilla (mantener Y original)
            Vector3 finalPos = currentPath[i].position;
            finalPos.y = transform.position.y;
            transform.position = finalPos;

            // Pequeña pausa entre casillas (opcional)
            yield return null;
        }

        state = State.Idle;
        OnMovementComplete?.Invoke();
    }

    void HighlightAllowedTiles(bool enable)
    {
        // Resalta (cambia color) las primeras 'allowedSteps' casillas de currentPath si tienen Renderer.
        // Implementación simple: modifica el color del material (cuidado con materiales compartidos).
        for (int i = 0; i < currentPath.Count; i++)
        {
            if (i >= allowedSteps && enable)
                continue;

            var rend = currentPath[i].GetComponent<Renderer>();
            if (rend != null)
            {
                if (enable)
                    rend.material.color = Color.yellow;
                else
                    rend.material.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Indica si el jugador está en medio de su movimiento o selección.
    /// </summary>
    public bool IsBusy()
    {
        return state != State.Idle;
    }

    // Método utilitario para cancelar (si lo necesitais)
    public void CancelMovement()
    {
        StopAllCoroutines();
        ExitSelectionMode();
    }
}